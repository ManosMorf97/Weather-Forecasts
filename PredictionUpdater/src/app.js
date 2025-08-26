let model = require("./models");
let APK=require("./api_keys");
const BinarySearchTree = require('@seald-io/binary-search-tree').AVLTree
//import { Op } from '@sequelize/core';

function callbackDate(){
    return new Date()
}
function CreateSQLDate(date){
    return date.getFullYear().toString()+"-"+(date.getMonth()+1).toString().padStart(2,'0')+
    "-"+date.getDate().toString().padStart(2,'0');
}
function CreateSQLTime(time){
    return time.getHours().toString().padStart(2,'0')+":"+time.getMinutes().toString().padStart(2,'0')+
    ":"+time.getSeconds().toString().padStart(2,'0');
}
/*async function UpdateUserNotifications(toBeUpdated){
    let notification_CST=toBeUpdated.map(x=>[x.Site_Id,x.City_Id,x.Timeslot_Id])
    await model.UserNotifications.update({"IsRead":false},{where:model.sequelize.where(model.sequelize.literal('(Site_Id,City_Id,Timeslot_Id)')),
        [model.Sequelize.Op.in]:notification_CST},{transaction:transaction})
}*/

async function AddUserNotifications(toBeInserted,transaction){
    let bst=new BinarySearchTree({compareKeys:uscKeyCompare})
    let uscs= await model.UserSiteCities.findAll({raw:true,transaction:transaction})
    for(const row of uscs){
        bst.insert({"Site_Id":row.Site_Id,"City_Id":row.City_Id},row.Email)
    }
    let userNotifications=[]
    for(const row of toBeInserted){
        let emails=bst.search({"Site_Id":row.Site_Id,"City_Id":row.City_Id})
        if(emails.length>0){
            for(const email of emails){
                userNotifications.push({"Email":email,"Site_Id":row.Site_Id,"City_Id":row.City_Id,"Timeslot_Id":row.Timeslot_Id,IsRead:false})
            }
        }
    }
    if(userNotifications.length>0)
        await model.UserNotifications.bulkCreate(userNotifications,{transaction:transaction})

}

async function UpdateTimeslots(today,transaction){
    let recent_Timeslot=await model.Timeslots.findOne({
        order:[
            ["Date","DESC"],
            ["Time","DESC"]
        ],
    transaction:transaction})
    let timeslotempty=recent_Timeslot===null
    let index_date=-1
    let times=["08","15","21"]
    let index_time=-1
    if(!timeslotempty){
        for(let i=0; i<3; i++){//index date most recent timeslot after 3 days
            let next_day=new Date(today.getFullYear(),today.getMonth(),today.getDate(),today.getHours(),today.getMinutes(),today.getSeconds())
            next_day.setDate(next_day.getDate()+i)
            if(String(recent_Timeslot.Date)==CreateSQLDate(next_day)){
                index_date=i
                break
            }
        }
        for(let i=0; i<3; i++){
            if(String(recent_Timeslot.Time).includes(times[i]+":00:00")){
                index_time=i
                break
            }
        }
    }
    let rows=[]
    if(index_time===2){
        index_date++
        index_time=0
    }
        
    else
        index_time++
    if(index_date===-1){
        index_date=0
        index_time=0
    }
    console.log("ID  "+index_date)
    for(let i=index_date; i<3; i++){
        let j=0
        if(i===index_date)
            j=index_time
        while(j<3){
            let next_day=new Date(today.getFullYear(),today.getMonth(),today.getDate(),today.getHours(),today.getMinutes(),today.getSeconds())
            next_day.setDate(today.getDate()+i)
            let next_day_for_db=CreateSQLDate(next_day)
            let next_time_for_db=times[j]+":00:00"
            rows.push({Date:next_day_for_db,Time:next_time_for_db})
            j++
        }
    }
    if (rows.length>0)
        await model.Timeslots.bulkCreate(rows,{transaction:transaction})
}
function InsideRange(begin_date,end_date,begin_time,end_time,day,hour){
    let date_Start=new Date(begin_date+"T"+begin_time)
    let date_Ending=new Date(end_date+"T"+end_time)
    let Date_to_Check=new Date(day+"T"+hour)
    if(date_Start<=Date_to_Check && Date_to_Check<=date_Ending)
        return true;
    else
        return false;

}
function BringCurrentPredictionsNotifications(citySites,timeslots,HashCities,HashSites,HashDateTimes,callback){
    let predictions=[]
    let dates=[]
    let alerts=[]
    let current_date=timeslots[0].Date;
    dates.push(current_date)
    for (const timeslot of timeslots){//ordered by date time
        if (current_date===timeslot.Date)
            continue
        dates.push(timeslot.Date)
        current_date=timeslot.Date
    }
    for (const citySite of citySites){
        let [predictionsW,alertsW]=callback(HashSites[citySite.Site_Id],HashCities[citySite.City_Id],dates,citySite.City_Id,citySite.Site_Id,HashDateTimes)
        predictions.push(...predictionsW)
        alerts.push(...alertsW)
    }
    return [predictions,alerts];
}
let KeyCompare=(a,b)=>{
    if(a.Site_Id>b.Site_Id) return 1;
    if(a.Site_Id<b.Site_Id) return -1;
    if(a.City_Id>b.City_Id) return 1;
    if(a.City_Id<b.City_Id) return -1;
    if(a.Timeslot_Id>b.Timeslot_Id) return 1;
    if(a.Timeslot_Id<b.Timeslot_Id) return -1;
    return 0;
}
var uscKeyCompare=(a,b)=>{
    if(a.Site_Id>b.Site_Id) return 1;
    if(a.Site_Id<b.Site_Id) return -1;
    if(a.City_Id>b.City_Id) return 1;
    if(a.City_Id<b.City_Id) return -1;
    return 0
}
async function BinaryTreeDB(table,transaction){
    let rows=await table.findAll({raw:true,transaction:transaction})
    const bst=new BinarySearchTree({compareKeys:KeyCompare})
    for(const row of rows){
        bst.insert({"Site_Id": row.Site_Id,"City_Id": row.City_Id,"Timeslot_Id": row.Timeslot_Id},row)
    }
    return bst

}

async function UpdateTablePredictions(predictions,predictionsDBT,transaction){
    let toBeUpdated=[]
    let toBeInserted=[]
    for(const prediction of predictions){
        let BTprediction=predictionsDBT.search({"Site_Id": prediction.Site_Id,"City_Id": prediction.City_Id,"Timeslot_Id": prediction.Timeslot_Id})
        if( BTprediction.length===0)
            toBeInserted.push(prediction)
        else if (BTprediction[0].Weather!==prediction.Weather||BTprediction[0].Danger!==prediction.Danger) 
            toBeUpdated.push(prediction)
    }
    console.log("LENE "+toBeUpdated.length)
    for(const prediction of toBeUpdated){
        console.log("UND "+prediction.Weather)
        let predictionRow=await model.Predictions.findOne({where:{"Site_Id":prediction.Site_Id,"City_Id":prediction.City_Id,"Timeslot_Id":prediction.Timeslot_Id},transaction:transaction})
        console.log("PRW "+predictionRow.Weather)
        predictionRow.Weather=prediction.Weather
        predictionRow.Danger=prediction.Danger
        try{
            await predictionRow.save({ transaction:transaction })
        }catch(error){
            console.log(error)
        }
    }
    let rows=toBeInserted.map((x)=>{return {
        "City_Id":x.City_Id,"Site_Id":x.Site_Id,
                "Timeslot_Id":x.Timeslot_Id,"Weather":x.Weather,"Danger":x.Danger}
    })
    try{
        await model.Predictions.bulkCreate(rows,{transaction:transaction})
    }catch(error){
        console.log(error)
    }

}
async function UpdateTableNotifications(notifications,notificationsDBT,today,transaction){
    let today_date=CreateSQLDate(today)
    let today_time=CreateSQLTime(today)
    let toBeUpdated=[]
    let toBeInserted=[]
    for(const notification of notifications){
        let BTnotification=notificationsDBT.search({"Site_Id":notification.Site_Id,"City_Id":notification.City_Id,"Timeslot_Id":notification.Timeslot_Id})
        if( BTnotification.length===0)
            toBeInserted.push(notification)
        else if (BTnotification[0].Description!==notification.Description)
            toBeUpdated.push(notification)    
    }
    for(const notification of toBeUpdated){
        let notificationRow=await model.Notifications.findOne({where:{"Site_Id":notification.Site_Id,"City_Id":notification.City_Id,
            "Timeslot_Id":notification.Timeslot_Id},transaction:transaction})
        notificationRow.Description='expired'
        //notificationRow.DateNotification=CreateSQLDate(today_date)
        //notificationRow.TimeNotification=CreateSQLTime(today_time)
        await notificationRow.save({transaction:transaction})
        
    }
    await model.Notifications.destroy({where:{"Description":'expired'},transaction:transaction})
    toBeInserted.push(...toBeUpdated)
    console.log("LEEN "+toBeUpdated.length)
    let rows=toBeInserted.map((x)=>{ return{
        "City_Id":x.City_Id,"Site_Id":x.Site_Id,
                "Timeslot_Id":x.Timeslot_Id,"Description":x.Description,
            "DateNotification":today_date,"TimeNotification":today_time}
    })
    try{
        await model.Notifications.bulkCreate(rows,{transaction:transaction})
    }catch(error){
        console.log(error)
    }
    await AddUserNotifications(toBeInserted,transaction)
}

function CreateAlert(today,alerts,day,time,add_symbol){
    let begin=alertAPI.efective
    let end=alertAPI.efective.expires
    let begin_date=begin.substring(0,begin.indexOf("T"))
    let end_date=end.substring(0,end.indexOf("T"))
    let begin_time=begin.substring(begin.indexOf("T")+1)
    let end_time=end.substring(end.indexOf("T")+1)
    if(add_symbol){
        begin_time=begin_time.substring(0,begin_time.indexOf("+"))
        end_time=end_time.substring(end_time.indexOf("+")+1)
    }
    if (InsideRange(begin_date,end_date,begin_time,end_time,day,time))
        alerts.push({"City_Id":city_id,"Site_Id":site_id,
    "Timeslot_Id":HashDateTimes[day.date][time.substring(lengthsub,time.length)].timeslot_id,"Description":alertAPI,
    "DateNotification":CreatSQLDate(today),
    "TimeNotification":CreateSQLTime(today)})


}
async function DeleteNoNeededNotifications(transaction){
    await model.sequelize.query(`delete notif from Notifications notif join Predictions p on `+ 
        `notif.Site_Id=p.Site_Id and notif.City_Id=p.City_Id and notif.Timeslot_Id=p.Timeslot_Id and p.Danger='false'`,{transaction:transaction})
}
async function main(callback,callbackDate){
    let transaction= await model.sequelize.transaction()
    console.log("TRANS "+transaction)
    let today=callbackDate()
    try{
        await UpdateTimeslots(today,transaction)
        let Cities=await model.Cities.findAll({transaction:transaction})

        let Sites=await model.Sites.findAll({transaction:transaction})
        
        let timeslots=await model.Timeslots.findAll({//for next predictions no past
            where:{
                date:{
                    [model.Sequelize.Op.gte]:CreateSQLDate(today)
                },
                time:{
                    [model.Sequelize.Op.gte]:CreateSQLTime(today)
                },
            },
             order:[
                    ["Date","ASC"],
                    ["Time","ASC"]
                ],
        transaction:transaction})
        let HashCities={}
        for(const city of Cities){
            HashCities[city.City_Id]=city.City_name
        }
        let HashSites={}
        for(const site of Sites){
            HashSites[site.Site_Id]=site.Site_name
        }
        let HashDateTimes={}
        for(const ts of timeslots){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslots){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslots){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

        let citySites=await model.CitySites.findAll({raw:true,transaction:transaction})
        let [predictions,notifications]=BringCurrentPredictionsNotifications(citySites,timeslots,HashCities,HashSites,HashDateTimes,callback)
        await UpdateTablePredictions(predictions,await BinaryTreeDB(model.Predictions,transaction),transaction)
        await DeleteNoNeededNotifications(transaction)
        await UpdateTableNotifications(notifications,await BinaryTreeDB(model.Notifications,transaction),today,transaction)
        console.log("EEEEEEEEEENNNNNNNNNNDDDDD")
        await transaction.commit()
    }catch(error){
        await transaction.rollback();
        console.log(error)
    }
    
}

function WeatherPredictions(site,location,dates,city_id,site_id,HashDateTime){

    async function VisualCrossing(location,dates,city_id,site_id,HashDateTimes){
        let predictions=[]
        let API_KEY=APK.VisualCrossing_API_KEY
        let response=await fetch(`https://weather.visualcrossing.com/`+
            `VisualCrossingWebServices/rest/services/timeline`+
            `/${location}/${dates[0]}/${dates[1]}/${dates[2]}}?key=${API_KEY}`,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        for (const day of [response.days[0],response.days[1],response.days[2]]){
            for (const timeweather of [response.hours[8],responsehours.hours[15],day.hours[21]]){
                predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":HashDateTimes[day.datetime][timeweather.datetime].timeslot_id,"Weather":timeweather,"Danger":false})
            }
        }
        let alerts=[]
        return {predictions,alerts}
        console.log(responsejson)
        return responsejson
        console.log(JSON.stringify(responsejson.days[0].hours[23]).length)
    }

    async function AccuWeather(location,dates,city_id,site_id,HashDateTimes){// all day same weather
        let predictions=[]
        let lengthsub="YYYY-MM-DD".length
        let API_KEY=APK.AccuWeather_API_KEY
        let response=await fetch(`http://dataservice.accuweather.com/locations/v1/cities/search`+
            `?apikey=${API_KEY}&q=${location}`,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        console.log(responsejson)
        let locationkey=responsejson[0].Key;
        console.log(responsejson)
        let responsekey=await fetch(`http://dataservice.accuweather.com/forecasts/v1/daily/5day/`+
            `${locationkey}?apikey=${API_KEY}&details=true&toplevel=true&metric=true`,{headers:{ 'accept':'application/json'}})
        let responsekeyjson=await responsekey.json();
        responsekeyjson.DailyForecasts[0]
        for(const dayweather of responsekeyjson.DailyForecasts){
            let my_date=dayweather.Date.substring(lengthsub)
            for(const time of HashDateTimes[my_date].keys()){
                predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":HashDateTimes[my_date][time.datetime].timeslot_id,"Weather":dayweather,"Danger":false})
            }
        }
        let alerts=[];
        return {predictions,alerts}
    }
    //AccuWeather('Athens','2025-8-1',APK.AccuWeather_API_KEY)
    async function WeatherApi(location,dates,city_id,site_id,HashDateTimes){// next two days keep me
        let lengthsub="YYYY-MM-DD ".length
        let alerts=[]
        let API_KEY=APK.Weatherapi_API_KEY
        let response=await fetch(`http://api.weatherapi.com/v1/forecast.json?key=${API_KEY}&q=${location}`+
            `&days=3&aqi=yes&alerts=yes`,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        let alertsFromAPI=responsejson.alerts.alert
        let forecastday=responsejson.forecastday///CHECK THIS
        for(const day of forecastday.slice(0,3)){
            let hour_forecasts=[day.hour[8],day.hour[15],day.hour[21]]
            for(const hour_forecast of hour_forecasts){
                let time=hour_forecast.hour.time
                predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":HashDateTimes[day.date][time.substring(lengthsub,time.length)].timeslot_id,"Weather":hour_forecast,"Danger":responsejson.alerts.alert.length>0})
                for(let alertAPI of alertsFromAPI){
                    if(!responsejson.location.region in alertAPI.areas)
                        continue
                    let today=new Date()
                    CreateAlert(today,alerts,day,time,true)
                }
            }
        }
        return {predictions,alerts}
        console.log(responsejson.forecast.forecastday[0].hour[0])
        console.log(responsejson.alerts.alert)
        
    }
    
    async function WeatherBit(location,dates,city_id,site_id,HashDateTimes){
        let predictions=[]
        let alerts=[]
        let API_KEY=APK.Weatherbit_API_KEY
        let response= await fetch(`https://api.weatherbit.io/v2.0/current?city=${location}&key=${API_KEY}`,
            {headers:{ 'accept':'application/json'}})
        let responsejson=response.json()
        let responsealert = await fetch(`https://api.weatherbit.io/v2.0/alerts?city=${location}&key=${API_KEY}`,
            {headers:{ 'accept':'application/json'}})
        let responsealertjson=responsealert.json()
        let alertsAPI=responsealertjson.alerts;
        for(const date of HashDateTimes.Keys()){
            for( const time of HashDateTimes.Keys()){
                predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":HashDateTimes[date][time].timeslot_id,"Weather":responsejson.data[0],"Danger":responsealertjson.alerts.length>0})
                for(const alertAPI of alertsAPI){
                    let today=new Date();
                    CreateAlert(today,date,time,alerts,false)
                }
            }
        }
        return {predictions,alerts}
        console.log(responsejson.data[0])
    }

    async function OpenMeteo(location,dates,city_id,site_id,HashDateTimes){
        let predictions=[]
         let response=await fetch(`https://geocoding-api.open-meteo.com/v1/search?name=${location}`
            ,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        let GeoPos=responsejson.results[0]
        let latitude=GeoPos.latitude
        let longitude=GeoPos.longitude
        let indexes=[8,15,21]
        for(let i=1; i<=2; i++)
            for(let j=0; j<4; j++)
                indexes.push(indexes[j]+24*i)
        console.log(responsejson)
        let responseloc=await fetch(`https://api.open-meteo.com/v1/forecast?`+
            `latitude=${latitude}&longitude=${longitude}`+
            `&hourly=temperature_2m&forecast_days=3`,{headers:{ 'accept':'application/json'}})
        let responselocjson=await responseloc.json();
        let temperature=responselocjson.hourly.temperature_2m
        for(const index of indexes){
            let datetime=responselocjson.hourly.time[index]
            let date=datetime.substring(0,datetime.indexOf("T"))
            let time=datetime.substring(datetime.indexOf("T")+1)
            predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":HashDateTimes[date][time].timeslot_id,"Weather":temperature[index]+"C","Danger":false})
        }

        console.log(responselocjson.hourly.temperature_2m[0])
    }
    switch (site){
        case "VisualCrossing":
            return VisualCrossing(location,dates,city_id,site_id,HashDateTime)
        case "AccuWeather":
            return AccuWeather(location,dates,city_id,site_id,HashDateTime)
        case "WeatherApi":
            return WeatherApi(location,dates,city_id,site_id,HashDateTime)
        case "WeatherBit":
            return WeatherBit(location,dates,city_id,site_id,HashDateTime)
        case "OpenMeteo":
            return OpenMeteo(location,dates,city_id,site_id,HashDateTime)
    
    }
        
    //WeatherApi("Athens","2025-8-1",APK.VisualCrossing_API_KEY)
}
//main(WeatherPredictions,callbackDate)
module.exports.main=main;


/*



*/