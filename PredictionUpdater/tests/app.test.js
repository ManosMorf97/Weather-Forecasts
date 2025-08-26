jest.setTimeout(90000)
let app=require('../src/app')
let model=require('../src/models')
let City_rows=null
let Site_rows=null
let site_names=null
let city_names=null

function CreateSQLDate(date){
    return date.getFullYear().toString()+"-"+(date.getMonth()+1).toString().padStart(2,'0')+
    "-"+date.getDate().toString().padStart(2,'0');
}
function CreateSQLTime(time){
    return time.getHours().toString().padStart(2,'0')+":"+time.getMinutes().toString().padStart(2,'0')+
    ":"+time.getSeconds().toString().padStart(2,'0');
}
let changerTestCase2=(Site_Name,City_Name,date,hour)=>{
    if(Site_Name==="AccuWeather"&&City_Name==="THESS"&&date==="2023-04-20"&&hour==="15:00:00")
        return " ZIG"
    if(Site_Name==="OpenMeteo"&&City_Name==="ATH"&&date==="2023-04-20"&&hour==="08:00:00")
        return " ZIG"
    return ""
}
beforeEach(async ()=>{
    City_rows=null
    Site_rows=null
    site_names=null
    city_names=null
    site_names=["AccuWeather","OpenMeteo","VisualCrossing","WeatheAPI","WeatherBit",]
    let sites=[]
    for(let site_name of site_names)
        sites.push({Site_name:site_name})
    await model.Sites.bulkCreate(sites)
    city_names=["ATH","THESS"]
    let cities=[]
    for(let city_name of city_names)
        cities.push({City_name:city_name})
    await model.Cities.bulkCreate(cities)
    let users=[{Email:"manos@gmail.com",Hashed_password:"pinhjs&h7384",Username:"manos"},
        {Email:"stelios@gmail.com",Hashed_password:"pinhjs&h7384",Username:"stelios"}]
    await model.Users.bulkCreate(users)
    City_rows=await model.Cities.findAll({ order:[
            ["City_name","ASC"],
        ],
        attributes:["City_Id"],
        raw: true})
    Site_rows=await model.Sites.findAll({ order:[
        ["Site_name","ASC"],
    ],
    attributes:["Site_Id"],
    raw: true})
    let citySites=[{City_Id:City_rows[0].City_Id,Site_Id:Site_rows[0].Site_Id},{City_Id:City_rows[0].City_Id,Site_Id:Site_rows[1].Site_Id},
    {City_Id:City_rows[0].City_Id,Site_Id:Site_rows[2].Site_Id},{City_Id:City_rows[1].City_Id,Site_Id:Site_rows[0].Site_Id},
    {City_Id:City_rows[1].City_Id,Site_Id:Site_rows[1].Site_Id},{City_Id:City_rows[1].City_Id,Site_Id:Site_rows[2].Site_Id}]
    await model.CitySites.bulkCreate(citySites)
    let UserSiteCities=[{Email:"manos@gmail.com",City_Id:City_rows[0].City_Id,Site_Id:Site_rows[0].Site_Id},
        {Email:"manos@gmail.com",City_Id:City_rows[0].City_Id,Site_Id:Site_rows[1].Site_Id},
        {Email:"manos@gmail.com",City_Id:City_rows[1].City_Id,Site_Id:Site_rows[0].Site_Id},
        {Email:"manos@gmail.com",City_Id:City_rows[1].City_Id,Site_Id:Site_rows[1].Site_Id},
        {Email:"stelios@gmail.com",City_Id:City_rows[0].City_Id,Site_Id:Site_rows[1].Site_Id},
        {Email:"stelios@gmail.com",City_Id:City_rows[0].City_Id,Site_Id:Site_rows[2].Site_Id},
        {Email:"stelios@gmail.com",City_Id:City_rows[1].City_Id,Site_Id:Site_rows[1].Site_Id},
        {Email:"stelios@gmail.com",City_Id:City_rows[1].City_Id,Site_Id:Site_rows[2].Site_Id}]
    await model.UserSiteCities.bulkCreate(UserSiteCities)
})


test("Testing Case 1",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let timeslots=[]
    let today=new Date()
    today.setFullYear(2023,3,20)
    for(let i=0; i<3; i++){
        for(let j=0; j<3; j++){
            timeslots.push({Date:CreateSQLDate(today),Time:hours[j]})
        }
        today.setDate(today.getDate()+1)
    }
    await model.Timeslots.bulkCreate(timeslots)
    let citysites=await model.CitySites.findAll()
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

    let predictions=[]
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=false
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour
                predictions.push(prediction)
            }
               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            let timeslot_pos=0
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    predictions.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                        Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,Danger:false,
                        Weather:"Site "+site_names[i]+" City "+city_names[j]+" Date "+datesStr[k]+" Time "+hours[l]})
                        timeslot_pos++
                }
            }
        }
    }
    await model.Predictions.bulkCreate(predictions)
    console.log(predictions.length)
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBeFalsy()
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l])
                    index++
                }
            }
        }
    }
    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
    let notifications_count=await model.Notifications.count()
    expect(notifications_count).toBeFalsy()
    
})

test("Testing Case 2",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let timeslots=[]
    let today=new Date()
    today.setFullYear(2023,3,20)
    for(let i=0; i<3; i++){
        for(let j=0; j<3; j++){
            timeslots.push({Date:CreateSQLDate(today),Time:hours[j]})
        }
        today.setDate(today.getDate()+1)
    }
    await model.Timeslots.bulkCreate(timeslots)
    let citysites=await model.CitySites.findAll()
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

    let predictions=[]
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=false
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour+
                    changerTestCase2(Site_Name,City_Name,date,hour)
                predictions.push(prediction)
            }
               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            let timeslot_pos=0
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    predictions.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                        Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,Danger:false,
                        Weather:"Site "+site_names[i]+" City "+city_names[j]+" Date "+datesStr[k]+" Time "+hours[l]})
                        timeslot_pos++
                }
            }
        }
    }
    await model.Predictions.bulkCreate(predictions)
    console.log(predictions.length)
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBeFalsy()
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l]+
                    changerTestCase2(site_names[i],city_names[j],datesStr[k],hours[l]))
                    index++
                }
            }
        }
    }
    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
    let notifications_count=await model.Notifications.count()
    expect(notifications_count).toBeFalsy()
    
    
})

test("Testing Case 3",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let timeslots=[]
    let today=new Date()
    today.setFullYear(2023,3,20)
    for(let i=0; i<2; i++){
        for(let j=0; j<3; j++){
            timeslots.push({Date:CreateSQLDate(today),Time:hours[j]})
        }
        today.setDate(today.getDate()+1)
    }
    await model.Timeslots.bulkCreate(timeslots)
    let citysites=await model.CitySites.findAll()
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

    let predictions=[]
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=false
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour
                predictions.push(prediction)
            }
               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            let timeslot_pos=0
            for(let k=0; k<2; k++){
                for(let l=0; l<3; l++){
                    predictions.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                        Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,Danger:false,
                        Weather:"Site "+site_names[i]+" City "+city_names[j]+" Date "+datesStr[k]+" Time "+hours[l]})
                        timeslot_pos++
                }
            }
        }
    }
    await model.Predictions.bulkCreate(predictions)
    console.log(predictions.length)
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBeFalsy()
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l])
                    index++
                }
            }
        }
    }
    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
    let notifications_count=await model.Notifications.count()
    expect(notifications_count).toBeFalsy()
    
    
})

test("Testing Case 4",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let timeslots=[]
    let today=new Date()
    today.setFullYear(2023,3,20)
    for(let i=0; i<3; i++){
        for(let j=0; j<3; j++){
            timeslots.push({Date:CreateSQLDate(today),Time:hours[j]})
        }
        today.setDate(today.getDate()+1)
    }
    await model.Timeslots.bulkCreate(timeslots)
    let citysites=await model.CitySites.findAll()
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

    let predictions=[]
    let alerts=[]
    let userNotifications=[]
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let dangerousity=false
                if ((Site_Name==="AccuWeather" && City_Name==="ATH" && date=="2023-04-21" && hour=="21:00:00")||
                    (Site_Name==="VisualCrossing" && City_Name==="THESS" && date=="2023-04-22" && hour=="15:00:00")){
                    dangerousity=true
                    let alert={}
                    alert['City_Id']=City_Id
                    alert['Site_Id']=Site_Id
                    alert['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                    alert['Description']=City_Name+" night fire "+Site_Name
                    alert['DateNotification']=CreateSQLDate(today)
                    alert['TimeNotification']=CreateSQLTime(today)
                    alerts.push(alert)

                }
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=dangerousity
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour
                predictions.push(prediction)

            }

               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    let emails=["manos@gmail.com","stelios@gmail.com"]
    let email_index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            let timeslot_pos=0
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if ((i==0 && j==0 && k==1 && l==2)||
                        (i==2 && j==1 && k==2 && l==1)){
                        dangerousity=true
                        alerts.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                            Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,
                            Desctiption:City_rows[j].City_name+" night fire "+Site_rows[i].Site_name,DateNotification:CreateSQLDate(today),
                            TimeNotification:CreateSQLTime(today)})
                        userNotifications.push({Email:emails[email_index],City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                            Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,IsRead:false})
                        email_index++
                    }
                    predictions.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                    Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,Danger:dangerousity,
                    Weather:"Site "+site_names[i]+" City "+city_names[j]+" Date "+datesStr[k]+" Time "+hours[l]})
                    timeslot_pos++
                }
            }
        }
    }
    await model.Predictions.bulkCreate(predictions)
    await model.Notifications.bulkCreate(alerts)
    await model.UserNotifications.bulkCreate(userNotifications)

    console.log(predictions.length)
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if ((site_names[i]==="AccuWeather" && city_names[j]==="ATH" && datesStr[k]=="2023-04-21" && hours[l]=="21:00:00")||
                        (site_names[i]==="VisualCrossing" && city_names[j]==="THESS" && datesStr[k]=="2023-04-22" && hours[l]=="15:00:00")){
                        dangerousity=true
                    }
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBe(dangerousity)
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l])
                    index++
                }
            }
        }
    }
    results=await model.sequelize.query(`select usno.Email,c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,n.Description from Notifications n  join UserNotifications usno on n.Site_Id=usno.Site_Id and n.City_Id=usno.City_Id `+
        `and n.Timeslot_Id=usno.Timeslot_Id join CitySites cs on n.Site_Id=cs.Site_Id `+
        `and n.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on n.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
        console.log("REL"+results)
        expect(results.length).toBe(2)
    let emailnotifs=["manos@gmail.com","stelios@gmail.com"]
    let sitenotifs=["AccuWeather","VisualCrossing"]
    let citynotifs=["ATH","THESS"]
    let datenotifs=["2023-04-21","2023-04-22"]
    let timenotifs=["21:00:00","15:00:00"]
    for(let i=0; i<2; i++){
        expect(results[i].Site_name).toBe(sitenotifs[i])
        expect(results[i].City_name).toBe(citynotifs[i])
        expect(results[i].Date).toBe(datenotifs[i])
        expect(results[i].Time).toBe(timenotifs[i])
        expect(results[i].Email).toBe(emailnotifs[i])
        expect(results[i].Description).toBe(citynotifs[i]+" night fire "+sitenotifs[i])
    }

    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
    
    
})


test("Testing Case 5",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let today=new Date()
    today.setFullYear(2023,3,20)
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let dangerousity=false
                if ((Site_Name==="AccuWeather" && City_Name==="ATH" && date=="2023-04-21" && hour=="21:00:00")||
                    (Site_Name==="VisualCrossing" && City_Name==="THESS" && date=="2023-04-22" && hour=="15:00:00")){
                    dangerousity=true
                    let alert={}
                    alert['City_Id']=City_Id
                    alert['Site_Id']=Site_Id
                    alert['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                    alert['Description']=City_Name+" night fire "+Site_Name
                    alert['DateNotification']=CreateSQLDate(today)
                    alert['TimeNotification']=CreateSQLTime(today)
                    alerts.push(alert)

                }
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=dangerousity
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour
                predictions.push(prediction)

            }

               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if ((site_names[i]==="AccuWeather" && city_names[j]==="ATH" && datesStr[k]=="2023-04-21" && hours[l]=="21:00:00")||
                        (site_names[i]==="VisualCrossing" && city_names[j]==="THESS" && datesStr[k]=="2023-04-22" && hours[l]=="15:00:00")){
                        dangerousity=true
                    }
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBe(dangerousity)
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l])
                    index++
                }
            }
        }
    }
    results=await model.sequelize.query(`select usno.Email,c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,n.Description from Notifications n  join UserNotifications usno on n.Site_Id=usno.Site_Id and n.City_Id=usno.City_Id `+
        `and n.Timeslot_Id=usno.Timeslot_Id join CitySites cs on n.Site_Id=cs.Site_Id `+
        `and n.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on n.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
        console.log("REL"+results)
        expect(results.length).toBe(2)
    let emailnotifs=["manos@gmail.com","stelios@gmail.com"]
    let sitenotifs=["AccuWeather","VisualCrossing"]
    let citynotifs=["ATH","THESS"]
    let datenotifs=["2023-04-21","2023-04-22"]
    let timenotifs=["21:00:00","15:00:00"]
    for(let i=0; i<2; i++){
        expect(results[i].Site_name).toBe(sitenotifs[i])
        expect(results[i].City_name).toBe(citynotifs[i])
        expect(results[i].Date).toBe(datenotifs[i])
        expect(results[i].Time).toBe(timenotifs[i])
        expect(results[i].Email).toBe(emailnotifs[i])
        expect(results[i].Description).toBe(citynotifs[i]+" night fire "+sitenotifs[i])
    }
    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
    
    
})

test("Testing Case 6",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let timeslots=[]
    let today=new Date()
    today.setFullYear(2023,3,20)
    for(let i=0; i<3; i++){
        for(let j=0; j<3; j++){
            timeslots.push({Date:CreateSQLDate(today),Time:hours[j]})
        }
        today.setDate(today.getDate()+1)
    }
    await model.Timeslots.bulkCreate(timeslots)
    let citysites=await model.CitySites.findAll()
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

    let predictions=[]
    let alerts=[]
    let userNotifications=[]
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let dangerousity=false
                if ((Site_Name==="AccuWeather" && City_Name==="ATH" && date=="2023-04-21" && hour=="21:00:00")||
                    (Site_Name==="VisualCrossing" && City_Name==="THESS" && date=="2023-04-22" && hour=="15:00:00")){
                    dangerousity=true
                    let alert={}
                    alert['City_Id']=City_Id
                    alert['Site_Id']=Site_Id
                    alert['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                    alert['Description']=City_Name+" night fire "+Site_Name
                    alert['DateNotification']=CreateSQLDate(today)
                    alert['TimeNotification']=CreateSQLTime(today)
                    alerts.push(alert)

                }
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=dangerousity
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour
                predictions.push(prediction)

            }

               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    let emails=["manos@gmail.com","stelios@gmail.com"]
    let email_index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            let timeslot_pos=0
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    predictions.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                    Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,Danger:dangerousity,
                    Weather:"Site "+site_names[i]+" City "+city_names[j]+" Date "+datesStr[k]+" Time "+hours[l]})
                    timeslot_pos++
                }
            }
        }
    }
    await model.Predictions.bulkCreate(predictions)

    console.log(predictions.length)
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if ((site_names[i]==="AccuWeather" && city_names[j]==="ATH" && datesStr[k]=="2023-04-21" && hours[l]=="21:00:00")||
                        (site_names[i]==="VisualCrossing" && city_names[j]==="THESS" && datesStr[k]=="2023-04-22" && hours[l]=="15:00:00")){
                        dangerousity=true
                    }
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBe(dangerousity)
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l])
                    index++
                }
            }
        }
    }
    results=await model.sequelize.query(`select usno.Email,c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,n.Description from Notifications n  join UserNotifications usno on n.Site_Id=usno.Site_Id and n.City_Id=usno.City_Id `+
        `and n.Timeslot_Id=usno.Timeslot_Id join CitySites cs on n.Site_Id=cs.Site_Id `+
        `and n.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on n.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
        console.log("REL"+results)
        expect(results.length).toBe(2)
    let emailnotifs=["manos@gmail.com","stelios@gmail.com"]
    let sitenotifs=["AccuWeather","VisualCrossing"]
    let citynotifs=["ATH","THESS"]
    let datenotifs=["2023-04-21","2023-04-22"]
    let timenotifs=["21:00:00","15:00:00"]
    for(let i=0; i<2; i++){
        expect(results[i].Site_name).toBe(sitenotifs[i])
        expect(results[i].City_name).toBe(citynotifs[i])
        expect(results[i].Date).toBe(datenotifs[i])
        expect(results[i].Time).toBe(timenotifs[i])
        expect(results[i].Email).toBe(emailnotifs[i])
        expect(results[i].Description).toBe(citynotifs[i]+" night fire "+sitenotifs[i])
    }
    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
})

test("Testing Case 7",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let timeslots=[]
    let today=new Date()
    today.setFullYear(2023,3,20)
    for(let i=0; i<3; i++){
        for(let j=0; j<3; j++){
            timeslots.push({Date:CreateSQLDate(today),Time:hours[j]})
        }
        today.setDate(today.getDate()+1)
    }
    await model.Timeslots.bulkCreate(timeslots)
    let citysites=await model.CitySites.findAll()
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

    let predictions=[]
    let alerts=[]
    let userNotifications=[]
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let dangerousity=false
                if ((Site_Name==="AccuWeather" && City_Name==="ATH" && date=="2023-04-21" && hour=="21:00:00")||
                    (Site_Name==="VisualCrossing" && City_Name==="THESS" && date=="2023-04-22" && hour=="15:00:00")){
                    dangerousity=true
                    let alert={}
                    alert['City_Id']=City_Id
                    alert['Site_Id']=Site_Id
                    alert['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                    alert['Description']=City_Name+" night fire "+Site_Name
                    if(Site_Name=="VisualCrossing")
                        alert['Description']=City_Name+" day fire "+Site_Name
                    alert['DateNotification']=CreateSQLDate(today)
                    alert['TimeNotification']=CreateSQLTime(today)
                    alerts.push(alert)

                }
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=dangerousity
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour
                predictions.push(prediction)

            }

               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    let emails=["manos@gmail.com","stelios@gmail.com"]
    let email_index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            let timeslot_pos=0
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if ((i==0 && j==0 && k==1 && l==2)||
                        (i==2 && j==1 && k==2 && l==1)){
                        dangerousity=true
                        alerts.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                            Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,
                            Desctiption:City_rows[j].City_name+" night fire "+Site_rows[i].Site_name,DateNotification:CreateSQLDate(today),
                            TimeNotification:CreateSQLTime(today)})
                        userNotifications.push({Email:emails[email_index],City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                            Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,IsRead:false})
                        email_index++
                    }
                    predictions.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                    Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,Danger:dangerousity,
                    Weather:"Site "+site_names[i]+" City "+city_names[j]+" Date "+datesStr[k]+" Time "+hours[l]})
                    timeslot_pos++
                }
            }
        }
    }
    await model.Predictions.bulkCreate(predictions)
    await model.Notifications.bulkCreate(alerts)
    await model.UserNotifications.bulkCreate(userNotifications)

    console.log(predictions.length)
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if ((site_names[i]==="AccuWeather" && city_names[j]==="ATH" && datesStr[k]=="2023-04-21" && hours[l]=="21:00:00")||
                        (site_names[i]==="VisualCrossing" && city_names[j]==="THESS" && datesStr[k]=="2023-04-22" && hours[l]=="15:00:00")){
                        dangerousity=true
                    }
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBe(dangerousity)
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l])
                    index++
                }
            }
        }
    }
    results=await model.sequelize.query(`select usno.Email,c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,n.Description from Notifications n  join UserNotifications usno on n.Site_Id=usno.Site_Id and n.City_Id=usno.City_Id `+
        `and n.Timeslot_Id=usno.Timeslot_Id join CitySites cs on n.Site_Id=cs.Site_Id `+
        `and n.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on n.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
        console.log("REL"+results)
        expect(results.length).toBe(2)
    let emailnotifs=["manos@gmail.com","stelios@gmail.com"]
    let sitenotifs=["AccuWeather","VisualCrossing"]
    let citynotifs=["ATH","THESS"]
    let datenotifs=["2023-04-21","2023-04-22"]
    let timenotifs=["21:00:00","15:00:00"]
    for(let i=0; i<2; i++){
        expect(results[i].Site_name).toBe(sitenotifs[i])
        expect(results[i].City_name).toBe(citynotifs[i])
        expect(results[i].Date).toBe(datenotifs[i])
        expect(results[i].Time).toBe(timenotifs[i])
        expect(results[i].Email).toBe(emailnotifs[i])
        if(sitenotifs[i]=="VisualCrossing")
            expect(results[i].Description).toBe(citynotifs[i]+" day fire "+sitenotifs[i])
        else
        expect(results[i].Description).toBe(citynotifs[i]+" night fire "+sitenotifs[i])
    }

    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
})
test("Testing Case 8",async ()=>{
    let hours=["08:00:00","15:00:00","21:00:00"]
    let timeslots=[]
    let today=new Date()
    today.setFullYear(2023,3,20)
    for(let i=0; i<3; i++){
        for(let j=0; j<3; j++){
            timeslots.push({Date:CreateSQLDate(today),Time:hours[j]})
        }
        today.setDate(today.getDate()+1)
    }
    await model.Timeslots.bulkCreate(timeslots)
    let citysites=await model.CitySites.findAll()
    let timeslot_rows=await model.Timeslots.findAll({order:[["Date","ASC"],["Time","ASC"]],raw:true})
    let HashDateTimes={}
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]={}
        }
        for(const ts of timeslot_rows){
            HashDateTimes[ts.Date][ts.Time]=ts
        }

    let predictions=[]
    let alerts=[]
    let userNotifications=[]
    let callback=(Site_Name,City_Name,dates,City_Id,Site_Id,HashDateTimes)=>{
        let predictions=[]
        let alerts=[]
        let hours=["08:00:00","15:00:00","21:00:00"]
        for (let date of dates)
            for(let hour of hours){
                let dangerousity=false
                if (Site_Name==="AccuWeather" && City_Name==="ATH" && date=="2023-04-21" && hour=="21:00:00"){
                    dangerousity=true
                    let alert={}
                    alert['City_Id']=City_Id
                    alert['Site_Id']=Site_Id
                    alert['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                    alert['Description']=City_Name+" night fire "+Site_Name
                    alert['DateNotification']=CreateSQLDate(today)
                    alert['TimeNotification']=CreateSQLTime(today)
                    alerts.push(alert)

                }
                let prediction={}
                prediction['City_Id']=City_Id
                prediction['Site_Id']=Site_Id
                prediction['Timeslot_Id']=HashDateTimes[date][hour].Timeslot_Id
                prediction['Danger']=dangerousity
                prediction['Weather']="Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour
                predictions.push(prediction)

            }

               // predictions.push({City_Id:City_Id,Site_Id:Site_Id,Timeslot_Id:HashDateTimes[date][hour].Timeslot_Id,Danger:false,Weather:
                //"Site "+Site_Name+" City "+City_Name+" Date "+date+" Time "+hour})
        return [predictions,alerts]
    }
    let datesStr=["2023-04-20","2023-04-21","2023-04-22"]
    let emails=["manos@gmail.com","stelios@gmail.com"]
    let email_index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            let timeslot_pos=0
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if ((i==0 && j==0 && k==1 && l==2)){
                        dangerousity=true
                        alerts.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                            Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,
                            Desctiption:City_rows[j].City_name+" night fire "+Site_rows[i].Site_name,DateNotification:CreateSQLDate(today),
                            TimeNotification:CreateSQLTime(today)})
                        userNotifications.push({Email:emails[email_index],City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                            Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,IsRead:false})
                        email_index++
                    }
                    predictions.push({City_Id:City_rows[j].City_Id,Site_Id:Site_rows[i].Site_Id,
                    Timeslot_Id:timeslot_rows[timeslot_pos].Timeslot_Id,Danger:dangerousity,
                    Weather:"Site "+site_names[i]+" City "+city_names[j]+" Date "+datesStr[k]+" Time "+hours[l]})
                    timeslot_pos++
                }
            }
        }
    }
    await model.Predictions.bulkCreate(predictions)
    await model.Notifications.bulkCreate(alerts)
    await model.UserNotifications.bulkCreate(userNotifications)

    console.log(predictions.length)
    let callbackDate=()=>{
        return new Date(2023,3,20,0,0,23)
    }
    await app.main(callback,callbackDate)
    let results=await model.sequelize.query(`select c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,p.Weather,p.Danger from Predictions p `+
        `join CitySites cs on p.Site_Id=cs.Site_Id `+
        `and p.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on p.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
    console.log(results)
    expect(results.length).toBe(54)
    let index=0
    for(let i=0; i<3; i++){
        for(let j=0; j<2; j++){
            for(let k=0; k<3; k++){
                for(let l=0; l<3; l++){
                    let dangerousity=false
                    if (site_names[i]==="AccuWeather" && city_names[j]==="ATH" && datesStr[k]=="2023-04-21" && hours[l]=="21:00:00"){
                        dangerousity=true
                    }
                    expect(results[index].Site_name).toBe(site_names[i])
                    expect(results[index].City_name).toBe(city_names[j])
                    expect(results[index].Date).toBe(datesStr[k])
                    expect(results[index].Time).toBe(hours[l])
                    expect(results[index].Danger).toBe(dangerousity)
                    expect(results[index].Weather).toBe("Site "+site_names[i]+" City "+city_names[j]+
                        " Date "+datesStr[k]+" Time "+hours[l])
                    index++
                }
            }
        }
    }
    results=await model.sequelize.query(`select usno.Email,c.City_name,s.Site_name,t.Date,CONVERT(VARCHAR(8), t.Time, 108) AS Time`+
        `,n.Description from Notifications n  join UserNotifications usno on n.Site_Id=usno.Site_Id and n.City_Id=usno.City_Id `+
        `and n.Timeslot_Id=usno.Timeslot_Id join CitySites cs on n.Site_Id=cs.Site_Id `+
        `and n.City_Id=cs.City_Id join Cities c on cs.City_Id=c.City_Id join Sites s on cs.Site_Id=s.Site_Id join `+
        `Timeslots t on n.Timeslot_Id=t.Timeslot_Id order by Site_name,City_name,Date,Time` ,{
        type: model.sequelize.QueryTypes.SELECT, // returns plain objects, not metadata
        raw: true})
        console.log("REL"+results)
        expect(results.length).toBe(1)
    let emailnotif="manos@gmail.com"
    let sitenotif="AccuWeather"
    let citynotif="ATH"
    let datenotif="2023-04-21"
    let timenotif="21:00:00"
        expect(results[0].Site_name).toBe(sitenotif)
        expect(results[0].City_name).toBe(citynotif)
        expect(results[0].Date).toBe(datenotif)
        expect(results[0].Time).toBe(timenotif)
        expect(results[0].Email).toBe(emailnotif)
        expect(results[0].Description).toBe(citynotif+" night fire "+sitenotif)

    let timeslots_count=await model.Timeslots.count()
    expect(timeslots_count).toBe(9)
    
    
})

afterEach(async()=>{
    await model.Users.destroy({where:{}})
    await model.Cities.destroy({where:{}})
    await model.Sites.destroy({where:{}})
    await model.Timeslots.destroy({where:{}})
})