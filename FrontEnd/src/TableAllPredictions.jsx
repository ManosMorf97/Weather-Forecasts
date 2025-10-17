import './styles.css'
import './decoration.css'
//import 'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css'
import url from './url.js'
import LoadingSpinner from './LoadingSpinner'
import Alert from './Alert.jsx'
import { useContext, useEffect, useState } from 'react'
import { updateStorage } from './utilities.js'
import {MyContext} from './Home.jsx'

function TableAllPredictions(){
    
    let [internalActive,setInternalActive]=useState(false)
    let [stateRatings,setStateRatings]=useState([])

    let [response,setResponse]=useState({status:-1,message:""})

    let context=useContext(MyContext)
    
    var appearAlert=(status,message)=>{
        setResponse(()=>({...response,status:status,message:message}))
        setTimeout(()=>{setResponse(()=>({...response,status:-1,message:message}))},5000)
    }
    var dealWithRating=async(y,x,index,semi_url='Predictions/AddRating')=>{
        setInternalActive(iac=>!iac)
        let data={
            'email':localStorage.getItem("UserLoggedIn"),
            "Site_Id":x.site_Id,
            "City_Id":x.city_Id,
            "Timeslot_Id":x.timeslot_Id,
            "Rating_Value":y
        }
        let res=await fetch(url+semi_url,{
            method: "POST",
            mode: "cors",
            headers:{
                'Content-Type':'application/json'
            },
            body:JSON.stringify(data)
        })
        let res_message=await res.json()
        let new_Rating=semi_url==='Predictions/AddRating'?y:0
        x.Rating_Value=new_Rating
        setStateRatings(st=>st.map((x2,index2)=>index2==index?new_Rating:x2))
        let storageNames=["usersPredictions","usersSuggestions"]
        let controllers=["Predictions/","Suggestions/"]
        updateStorage(controllers,storageNames,url)
        setInternalActive(iac=>!iac)
        appearAlert(res.status,res_message)
    }

    useEffect(()=>{
        let predictionsString=localStorage.getItem("usersPredictions")
        if(predictionsString==null){
            console.log("LLLL")
            return
        }
        console.log("PSK")
        setStateRatings(()=>JSON.parse(predictionsString).filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time)).map(x=>x.rating_Value))
    },[context.predictions])

    
    return(
        <>

            <LoadingSpinner active={internalActive}></LoadingSpinner>
            <Alert status={response.status} message={response.message}></Alert>
            <table className={(context.active?"non-displayed ":" ")+(internalActive?"sleeping ":" ")+"table-centered table table-dark table-bordered z-1"}>
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
                    {context.predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time)).
                    length>0?
                        context.predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time))
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
                                        <span key={index2} className="bi bi-star-fill " onClick={()=>dealWithRating(y,x,index)}></span>:
                                        <span key={index2} className="bi bi-star " onClick={()=>dealWithRating(y,x,index)}></span>)
                                    }
                                    {stateRatings[index]>0?
                                        <button className="bg-danger" onClick={(e)=>
                                        {e.preventDefault();
                                        dealWithRating(stateRatings[index],x,index,'Predictions/DeleteRating')}
                                        } >
                                            Delete Rating
                                        </button>:
                                        <span></span>
                                    }
                                </td>
                            </tr>
                            )
                        }
                            
                        
                    
                        ):<tr></tr>
                    }
                </tbody>
            </table>
        </>
    )
}

export default TableAllPredictions;