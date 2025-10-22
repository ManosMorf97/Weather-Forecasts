let APK=require("./api_keys");
let predictor=require("./predictor");
function callbackDate(){
    return new Date()
}


function WeatherPredictions(site,location,dates,city_id,site_id,HashDateTime){

    function InsideRange(begin_date,end_date,begin_time,end_time,day,hour){
        let date_Start=new Date(begin_date+"T"+begin_time)
        let date_Ending=new Date(end_date+"T"+end_time)
        let Date_to_Check=new Date(day+"T"+hour)
        if(date_Start<=Date_to_Check && Date_to_Check<=date_Ending)
            return true;
        else
            return false;

    }

    function CreateSQLDate(date){
        return date.getFullYear().toString()+"-"+(date.getMonth()+1).toString().padStart(2,'0')+
        "-"+date.getDate().toString().padStart(2,'0');
    }

    function CreateSQLTime(time){
        return time.getHours().toString().padStart(2,'0')+":"+time.getMinutes().toString().padStart(2,'0')+
        ":"+time.getSeconds().toString().padStart(2,'0');
    }

    function CreateAlert(today,alerts,day,time,alertjson,Timeslot_Id,add_symbol){
        let returned=false
        let begin=null
        let end=null
        if(add_symbol){
            begin=alertjson.efective
            end=alertjson.efective.expires
        }else{
            begin=alertjson.onset
            end=alertjson.ends
        }
        let begin_date=begin.substring(0,begin.indexOf("T"))
        let end_date=end.substring(0,end.indexOf("T"))
        let begin_time=begin.substring(begin.indexOf("T")+1)
        let end_time=end.substring(end.indexOf("T")+1)
        if(add_symbol){
            begin_time=begin_time.substring(0,begin_time.indexOf("+"))
            end_time=end_time.substring(end_time.indexOf("+")+1)
        }
        returned=InsideRange(begin_date,end_date,begin_time,end_time,day,time)
        if (returned)
            alerts.push({"City_Id":city_id,"Site_Id":site_id,
            "Timeslot_Id":Timeslot_Id,"Description":alertjson.description,
            "DateNotification":CreateSQLDate(today),
            "TimeNotification":CreateSQLTime(today)})
        return returned
        


}
    async function VisualCrossing(location,dates,city_id,site_id,HashDateTimes){
        let predictions=[]
        let alerts=[]
        let API_KEY=APK.VisualCrossing_API_KEY
        let response=await fetch(`https://weather.visualcrossing.com/`+
            `VisualCrossingWebServices/rest/services/timeline`+
            `/${location}/${dates[0]}/${dates[2]}?key=${API_KEY}`,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        for (const day of [responsejson.days[0],responsejson.days[1],responsejson.days[2]]){
            for (const timeweather of [day.hours[8],day.hours[15],day.hours[21]]){
                let Timeslot=HashDateTimes[day.datetime][timeweather.datetime]
                if(Timeslot==null)
                    continue
                let weatherObject={'dayNight':timeweather.icon.includes("day")?'day':'night','temperature':timeweather.temp,
                    'feelsLike':timeweather.feelslike,
                    'windSpeed':timeweather.windspeed,'skyCondition':timeweather.conditions,'humidity':timeweather.humidity
                }
                let danger_exists=false
                if(responsejson.alerts!=null && responsejson.alerts.length>0)
                    danger_exists=CreateAlert(new Date(),alerts,day.datetime,timeweather.datetime,responsejson.alerts[0],Timeslot.Timeslot_Id,false)
                predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":Timeslot.Timeslot_Id,"Weather":JSON.stringify(weatherObject),"Danger":danger_exists})//FIXME
                
            }
        }
        
        return [predictions,alerts]
    }
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
                let weatherObject={'dayNight':hour_forecast.condition.icon.includes("day")?'day':'night','temperature':hour_forecast.temp_c,
                    'feelsLike':hour_forecast.feelslike_c,'windSpeed':hour_forecast.wind_kph,
                    'skyCondition':hour_forecast.condition.text,'humidity':hour_forecast.humidity
                }
                prediction["Weather"]=JSON.stringify(weatherObject)
                prediction["Danger"]=responsejson.alerts.alert>0
                predictions.push(prediction)
                for(let alertAPI of alertsFromAPI){
                    if(!alertAPI.areas.includes(responsejson.location.region))
                        continue
                    let today=new Date()
                    //CreateAlert(today,alerts,day,time,alertAPI,true)
                }
            }
        }
        return [predictions,alerts]
        
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
            for(let j=0; j<3; j++)
                indexes.push(indexes[j]+24*i)
        let responseloc=await fetch(`https://api.open-meteo.com/v1/forecast?`+
            `latitude=${latitude}&longitude=${longitude}`+
            `&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m,is_day,apparent_temperature,weather_code&forecast_days=3`,
            {headers:{ 'accept':'application/json'}})
        let responselocjson=await responseloc.json();
        let forecast=responselocjson.hourly
        for(const index of indexes){
            let datetime=responselocjson.hourly.time[index]
            let date=datetime.substring(0,datetime.indexOf("T"))
            let time=datetime.substring(datetime.indexOf("T")+1)
            if(HashDateTimes[date]==null)
                continue
            let Timeslot=HashDateTimes[date][time+":00"]
            if(Timeslot==null)
                continue
            let weatherObject={'dayNight':forecast.is_day[index]==1?'day':'night','temperature':forecast.temperature_2m[index],
                'feelsLike':forecast.apparent_temperature[index],'windSpeed':forecast.wind_speed_10m[index],
                'skyCondition':forecast.weather_code[index],'humidity':forecast.relative_humidity_2m[index]
            }
            predictions.push({"City_Id":city_id,"Site_Id":site_id,
                "Timeslot_Id":Timeslot.Timeslot_Id,"Weather":JSON.stringify(weatherObject),"Danger":false})
        }
        return [predictions,alerts]
    }

    async function OpenWeatherMap(location,dates,city_id,site_id,HashDateTimes){
        let predictions=[]
        let alerts=[]
        let API_KEY=APK.OpenWeatherMap_API_KEY
        let response=await fetch(`https://api.openweathermap.org/geo/1.0/direct?q=${location}&limit=1&appid=${API_KEY}`
            ,{headers:{ 'accept':'application/json'}})
        let responsejson=await response.json();
        let latitude=responsejson[0].lat;
        let longitude=responsejson[0].lon;
        let responseloc=await fetch(`https://api.openweathermap.org/data/2.5/forecast?`+
            `lat=${latitude}&lon=${longitude}&appid=${API_KEY}&units=metric`,
            {headers:{ 'accept':'application/json'}})
        let responselocjson=await responseloc.json();
        for(const element of responselocjson.list){
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
            let weatherObject={'dayNight':element.sys.pod=='d'?'day':'night','temperature':element.main.temp,'feelsLike':element.main.feels_like,
                'windSpeed':element.wind.speed,'skyCondition':element.weather[0].description,'humidity':element.main.humidity,
            }
            let weather=JSON.stringify(weatherObject)
           
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