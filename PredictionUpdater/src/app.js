let APK=require("./api_keys");
let predictor=require("./predictor");
function callbackDate(){
    return new Date()
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
                "Timeslot_Id":HashDateTimes[day.datetime][timeweather.datetime].Timeslot_Id,"Weather":timeweather,"Danger":false})
            }
        }
        let alerts=[]
        return [predictions,alerts]
        console.log(responsejson)
        return responsejson
        console.log(JSON.stringify(responsejson.days[0].hours[23]).length)
    }

    /*async function AccuWeather(location,dates,city_id,site_id,HashDateTimes){// all day same weather
        let predictions=[]
        let lengthsub="YYYY-MM-DD".length
        let API_KEY=APK.AccuWeather_API_KEY
        console.log("TTTT"+API_KEY)
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
    }*/
    //AccuWeather('Athens','2025-8-1',APK.AccuWeather_API_KEY)
    async function WeatherApi(location,dates,city_id,site_id,HashDateTimes){// next two days keep me
        let lengthsub="YYYY-MM-DD ".length
        let alerts=[]
        let predictions=[]
        let API_KEY=APK.Weatherapi_API_KEY
        let response=await fetch(`http://api.weatherapi.com/v1/forecast.json?key=${API_KEY}&q=${location}`+
            `&days=3&aqi=yes&alerts=yes`,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        let alertsFromAPI=responsejson.alerts.alert
        let forecastday=responsejson.forecast.forecastday///CHECK THIS
        for (const day in HashDateTimes)
            for(const time in HashDateTimes[day])
                console.log(HashDateTimes[day][time].Date+" "+HashDateTimes[day][time].Time)
        for(const day of forecastday.slice(0,3)){
            let hour_forecasts=[day.hour[8],day.hour[15],day.hour[21]]
            for(const hour_forecast of hour_forecasts){
                let time=hour_forecast.time
                let Timeslot=HashDateTimes[day.date][time.substring(lengthsub,time.length)+":00"]
                if(Timeslot==null)
                    continue
                console.log("DT "+day.date+" "+time.substring(lengthsub,time.length)+":00")
                let prediction={}
                prediction["City_Id"]=city_id
                prediction["Site_Id"]=site_id
                prediction["Timeslot_Id"]=Timeslot.Timeslot_Id
                prediction["Weather"]=hour_forecast
                prediction["Danger"]=responsejson.alerts.alert>0
                predictions.push(prediction)
                console.log(prediction)
                for(let alertAPI of alertsFromAPI){
                    if(!alertAPI.areas.includes(responsejson.location.region))
                        continue
                    let today=new Date()
                    CreateAlert(today,alerts,day,time,true)
                }
            }
        }
        console.log(alerts.length)
        return [predictions,alerts]
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
                "Timeslot_Id":HashDateTimes[date][time].Timeslot_Id,"Weather":responsejson.data[0],"Danger":responsealertjson.alerts.length>0})
                for(const alertAPI of alertsAPI){
                    let today=new Date();
                    CreateAlert(today,date,time,alerts,false)
                }
            }
        }
        return [predictions,alerts]
        console.log(responsejson.data[0])
    }

    async function OpenMeteo(location,dates,city_id,site_id,HashDateTimes){
        let predictions=[]
        let alerts=[]
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
                "Timeslot_Id":HashDateTimes[date][time].Timeslot_Id,"Weather":temperature[index]+"C","Danger":false})
        }
        return [predictions,alerts]
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
predictor.main(WeatherPredictions,callbackDate)


/*



*/