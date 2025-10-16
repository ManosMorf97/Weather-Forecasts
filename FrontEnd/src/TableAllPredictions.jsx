import './styles.css'
import './decoration.css'
//import 'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css'
import url from './url.js'
import LoadingSpinner from './LoadingSpinner'
import { useEffect, useState } from 'react'
import { updateStorage } from './utilities.js'

function TableAllPredictions(props){
    
    let [internalActive,setInternalActive]=useState(false)
    let [stateRatings,setStateRatings]=useState([])

    var rateWeather=async(y,x,index)=>{
        setInternalActive(iac=>!iac)
        let data={
            'email':localStorage.getItem("UserLoggedIn"),
            "Site_Id":x.site_Id,
            "City_Id":x.city_Id,
            "Timeslot_Id":x.timeslot_Id,
            "Rating_Value":y
        }
        await fetch(url+'Predictions/AddRating',{
            method: "POST",
            mode: "cors",
            headers:{
                'Content-Type':'application/json'
            },
            body:JSON.stringify(data)
        })
        x.Rating_Value=y
        setStateRatings(st=>st.map((x2,index2)=>index2==index?y:x2))
        let storageNames=["usersPredictions","usersSuggestions"]
        let controllers=["Predictions/","Suggestions/"]
        updateStorage(controllers,storageNames,url)
        setInternalActive(iac=>!iac)
    }
    useEffect(()=>{
        let predictionsString=localStorage.getItem("usersPredictions")
        if(predictionsString==null){
            console.log("LLLL")
            return
        }
        console.log("PSK")
        setStateRatings(()=>JSON.parse(predictionsString).filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time)).map(x=>x.rating_Value))
    },[props.predictions])
    return(
        <>
            <LoadingSpinner active={internalActive}></LoadingSpinner>
            <table className={(props.active?"non-displayed ":" ")+(internalActive?"sleeping ":" ")+"table-centered table table-dark table-bordered z-1"}>
                <thead>
                    <tr>
                        <th>Site Name</th>
                        <th>City Name</th>
                        <th>Date</th>
                        <th>Time</th>
                        <th>DayNight</th>
                        <th>Temperature</th>
                        <th>Temperature Feels</th>
                        <th>Wind Speed</th>
                        <th>Day Or Night</th>
                        <th>Humidity</th>
                        <th>Rating Value</th>
                    </tr>
                    
                </thead>
                <tbody>
                    {props.predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time)).
                    length>0?
                    props.predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time))
                    .map((x,index)=>{
                        let weather=JSON.parse(x.weather)
                        return(
                        <tr key={index}>
                            <td>{x.site_name}</td>
                            <td>{x.city_name}</td>
                            <td>{x.date}</td>
                            <td>{x.time}</td>
                            <td>{weather.dayNight}</td>
                            <td>{weather.temperature}</td>
                            <td>{weather.feelsLike}</td>
                            <td>{weather.windSpeed}</td>
                            <td>{weather.skyCondition}</td>
                            <td>{weather.humidity}</td>
                            <td>
                                {[1,2,3,4,5].map((y,index2)=>y<=stateRatings[index]?
                                    <span key={index2} className="bi bi-star-fill " onClick={()=>rateWeather(y,x,index)}></span>:
                                    <span key={index2} className="bi bi-star " onClick={()=>rateWeather(y,x,index)}></span>)}
                                {stateRatings[index]>0?<button className="bg-danger">Delete Rating</button>:<span></span>}
                            </td>
                        </tr>
                        )
                        


                    }
                        
                    
                
                    ):<tr></tr>}
                </tbody>
            </table>
        </>
    )
}

export default TableAllPredictions;