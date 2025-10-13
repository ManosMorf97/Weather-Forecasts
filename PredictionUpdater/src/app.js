let APK=require("./api_keys");
let predictor=require("./predictor");
function callbackDate(){
    return new Date()
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
        console.log("VisualCrossing!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
        let predictions=[]
        let API_KEY=APK.VisualCrossing_API_KEY
        let response=await fetch(`https://weather.visualcrossing.com/`+
            `VisualCrossingWebServices/rest/services/timeline`+
            `/${location}/${dates[0]}/${dates[2]}?key=${API_KEY}`,{headers:{ 'accept':'application/json'}})
        console.log(response)
        let responsejson=await response.json();
        for(const key in HashDateTimes){
            for(const subkey in HashDateTimes[key]){
                console.log("KKK "+key+" "+subkey)
            }
        }
        for (const day of [responsejson.days[0],responsejson.days[1],responsejson.days[2]]){
            for (const timeweather of [day.hours[8],day.hours[15],day.hours[21]]){
                let Timeslot=HashDateTimes[day.datetime][timeweather.datetime]
                if(Timeslot==null)
                    continue
                let weatherObject={'dayNight':timeweather.icon,'temperature':timeweather.temp,'feelsLike':timeweather.feelslike,
                    'windSpeed':timeweather.windspeed,'skyCondition':timeweather.conditions,'humidity':timeweather.humidity
                }
                predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":Timeslot.Timeslot_Id,"Weather":JSON.stringify(timeweather),"Danger":false})
            }
        }
        let alerts=[]
        return [predictions,alerts]
        console.log(responsejson)
        return responsejson
        console.log(JSON.stringify(responsejson.days[0].hours[23]).length)
    }
    async function WeatherApi(location,dates,city_id,site_id,HashDateTimes){// next two days keep me
        console.log("WeatherAPI!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
        let lengthsub="YYYY-MM-DD ".length
        let alerts=[]
        let predictions=[]
        let API_KEY=APK.Weatherapi_API_KEY
        let response=await fetch(`http://api.weatherapi.com/v1/forecast.json?key=${API_KEY}&q=${location}`+
            `&days=3&aqi=yes&alerts=yes`,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        let alertsFromAPI=responsejson.alerts.alert
        let forecastday=responsejson.forecast.forecastday///CHECK THIS
        for(const day of forecastday.slice(0,3)){
            let hour_forecasts=[day.hour[8],day.hour[15],day.hour[21]]
            for(const hour_forecast of hour_forecasts){
                let time=hour_forecast.time
                let Timeslot=HashDateTimes[day.date][time.substring(lengthsub,time.length)+":00"]
                if(Timeslot==null)
                    continue
                let prediction={}
                prediction["City_Id"]=city_id
                prediction["Site_Id"]=site_id
                prediction["Timeslot_Id"]=Timeslot.Timeslot_Id
                let weatherObject={'dayNight':time.condition.icon,'temperature':time.temp_c,'feelsLike':time.feels_like_c,
                    'windSpeed':time.wind_kph,'skyCondition':time.condition.text,'humidity':time.humidity
                }
                prediction["Weather"]=JSON.stringify(hour_forecast)
                prediction["Danger"]=responsejson.alerts.alert>0
                predictions.push(prediction)
                for(let alertAPI of alertsFromAPI){
                    if(!alertAPI.areas.includes(responsejson.location.region))
                        continue
                    let today=new Date()
                    CreateAlert(today,alerts,day,time,true)
                }
            }
        }
        return [predictions,alerts]
        console.log(responsejson.forecast.forecastday[0].hour[0])
        console.log(responsejson.alerts.alert)
        
    }

    async function OpenMeteo(location,dates,city_id,site_id,HashDateTimes){
        console.log("OpenMeteo!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
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
            for(let j=0; j<3; j++)
                indexes.push(indexes[j]+24*i)
        let responseloc=await fetch(`https://api.open-meteo.com/v1/forecast?`+
            `latitude=${latitude}&longitude=${longitude}`+
            `&hourly==temperature_2m,relative_humidity_2m,wind_speed_10m,is_day,apparent_temperature,weather_code&forecast_days=3`,
            {headers:{ 'accept':'application/json'}})
        let responselocjson=await responseloc.json();
        console.log(responselocjson)
        let forecast=responselocjson.hourly
        for(const index of indexes){
            let datetime=responselocjson.hourly.time[index]
            console.log(datetime)
            let date=datetime.substring(0,datetime.indexOf("T"))
            let time=datetime.substring(datetime.indexOf("T")+1)
            if(HashDateTimes[date]==null)
                continue
            let Timeslot=HashDateTimes[date][time+":00"]
            if(Timeslot==null)
                continue
            let weatherObject={'dayNight':forecast.is_day[index],'temperature':forecast.temperature_2m[index],
                'feelsLike':forecast.apparent_temperature[index],'windSpeed':forecast.wind_speed_10m[index],
                'skyCondition':forecast.weather_code[index],'humidity':forecast.relative_humidity_2m[index]
            }
            predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":Timeslot.Timeslot_Id,"Weather":forecast.temperature_2m[index]+"C","Danger":false})
        }
        return [predictions,alerts]
        console.log(responselocjson.hourly.temperature_2m[0])
    }

    async function OpenWeatherMap(location,dates,city_id,site_id,HashDateTimes){
        console.log("OpenWeatherMap!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
        let predictions=[]
        let alerts=[]
        let API_KEY=APK.OpenWeatherMap_API_KEY
        let response=await fetch(`https://api.openweathermap.org/geo/1.0/direct?q=${location}&limit=1&appid=${API_KEY}`
            ,{headers:{ 'accept':'application/json'}})
        console.log("RRRRRR"+response)
        let responsejson=await response.json();
        console.log("LOCATION "+location)
        let latitude=responsejson[0].lat;
        let longitude=responsejson[0].lon;
        let responseloc=await fetch(`https://api.openweathermap.org/data/2.5/forecast?`+
            `lat=${latitude}&lon=${longitude}&appid=${API_KEY}&units=metric`,
            {headers:{ 'accept':'application/json'}})
        let responselocjson=await responseloc.json();
        for(element of responselocjson.list){
            console.log("EL "+element)
            let datetime=element.dt_txt
            let date=datetime.substring(0,datetime.indexOf(" "))
            let time=datetime.substring(datetime.indexOf(" ")+1)
             if(time.startsWith("09"))
                time="08:00:00"
             if(HashDateTimes[date]==null)
                continue
            
            let Timeslot=HashDateTimes[date][time]
            if(Timeslot==null)
                continue
            let weatherObject={'dayNight':element.sys.pod,'temperature':element.main.temp,'feelsLike':element.main.feels_like,
                'windSpeed':element.wind.speed,'skyCondition':element.weather[0].description,'humidity':element.main.humidity,
            }
            let weather=JSON.stringify(element.weather[0])
            console.log("WWW "+weather)
            
           
            console.log(date)
           
            predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":Timeslot.Timeslot_Id,"Weather":weather,"Danger":false})
        }
        //0 2 4


        return [predictions,alerts]

    }
    switch (site){
        case "VisualCrossing":
            return VisualCrossing(location,dates,city_id,site_id,HashDateTime)
        case "WeatherApi":
            return WeatherApi(location,dates,city_id,site_id,HashDateTime)
        case "OpenWeatherMap":
            return OpenWeatherMap(location,dates,city_id,site_id,HashDateTime)
        case "OpenMeteo":
            return OpenMeteo(location,dates,city_id,site_id,HashDateTime)
    
    }
        
    //WeatherApi("Athens","2025-8-1",APK.VisualCrossing_API_KEY)
}
predictor.main(WeatherPredictions,callbackDate)


/*
celsius air sun/cloud/snow humidity pollen day/night  real feel


*/