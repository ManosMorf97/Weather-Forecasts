let model = require("./models");

const BinarySearchTree = require('@seald-io/binary-search-tree').AVLTree
//import { Op } from '@sequelize/core';

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

async function BringCurrentPredictionsNotifications(citySites,timeslots,HashCities,HashSites,HashDateTimes,callback){
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
        let [predictionsW,alertsW]=await callback(HashSites[citySite.Site_Id],HashCities[citySite.City_Id],dates,citySite.City_Id,citySite.Site_Id,HashDateTimes)
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
    for(const prediction of toBeUpdated){
        let predictionRow=await model.Predictions.findOne({where:{"Site_Id":prediction.Site_Id,"City_Id":prediction.City_Id,"Timeslot_Id":prediction.Timeslot_Id},transaction:transaction})
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

async function DeleteNoNeededNotifications(transaction){
    await model.sequelize.query(`delete notif from Notifications notif join Predictions p on `+ 
        `notif.Site_Id=p.Site_Id and notif.City_Id=p.City_Id and notif.Timeslot_Id=p.Timeslot_Id and p.Danger='false'`,{transaction:transaction})
}
async function main(callback,callbackDate){
    let transaction= await model.sequelize.transaction()
    let today=callbackDate()
    try{
        await UpdateTimeslots(today,transaction)
        let Cities=await model.Cities.findAll({transaction:transaction})

        let Sites=await model.Sites.findAll({transaction:transaction})
        
        let timeslots=await model.Timeslots.findAll({//for next predictions no past
            where:{
                [model.Sequelize.Op.or]:[{
                    date:{
                        [model.Sequelize.Op.gt]:CreateSQLDate(today)
                    }
                },
                {
                    
                        date:{
                            [model.Sequelize.Op.eq]:CreateSQLDate(today)
                        },time:{
                            [model.Sequelize.Op.gte]:CreateSQLTime(today)
                        }
                    
                }]
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
        let [predictions,notifications]=await BringCurrentPredictionsNotifications(citySites,timeslots,HashCities,HashSites,HashDateTimes,callback)
        await UpdateTablePredictions(predictions,await BinaryTreeDB(model.Predictions,transaction),transaction)
        await DeleteNoNeededNotifications(transaction)
        await UpdateTableNotifications(notifications,await BinaryTreeDB(model.Notifications,transaction),today,transaction)
        await transaction.commit()
    }catch(error){
        await transaction.rollback();
        console.log(error)
    }
    
}
//main(WeatherPredictions,callbackDate)
module.exports={main,CreateSQLDate,CreateSQLTime};


/*



*/