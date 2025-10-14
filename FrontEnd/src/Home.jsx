import { useEffect,useState,useRef } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import LoadingSpinner from './LoadingSpinner.jsx'
import HomeNav from "./HomeNav.jsx";
import './styles.css'
import './decoration.css'

function Home(){
    
    let navigate=useNavigate()
    let [active,activate]=useState(true)
    let [email,setEmail]=useState("")
    let [predictions,setPredictions]=useState([])
    let storageNames=useRef(["usersPredictions","usersSuggestions","usersNotification","usersCountNotification"])
    var fetchUserData=async (url)=>{
        
        let emptyStorage=false
        for (let storageName of storageNames.current)
            if(localStorage.getItem(storageName)==null)
                emptyStorage=true
        if(!emptyStorage){
            setPredictions(JSON.parse(localStorage.getItem(storageNames.current[0])))
            activate(false)
            return
        }
        let controllers=["Predictions/","Suggestions/","Notifications/","Notifications/GetCountNotifications/"]
        for(let i=0; i<controllers.length; i++){
            var res= await fetch(url+controllers[i],{
                method: "POST",
                mode: "cors",
                headers:{
                    'Content-Type':'application/json'
                },
                body:JSON.stringify(localStorage.getItem("UserLoggedIn"))
            })
            let resjson= JSON.stringify(await res.json())
            localStorage.setItem(storageNames.current[i],resjson)
            setPredictions(JSON.parse(localStorage.getItem(storageNames.current[0])))
        }
        
        activate(false)
    }

   
    useEffect(()=>{
        console.log(localStorage.getItem("UserLoggedIn"))
        if(localStorage.getItem("UserLoggedIn")==null)
            navigate('/logIn')
        else{
            setEmail(localStorage.getItem("UserLoggedIn"))
            fetchUserData(url)
        }
            
    },[])
    
    return(<>
        <LoadingSpinner active={active} />
        <div>
            <HomeNav active={active} email={email}/>
            <table className="table-centered table table-dark table-bordered z-1">
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
                    </tr>
                    
                </thead>
                <tbody>
                    {predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time)).
                    length>0?
                    predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time))
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
                        </tr>
                        )
                        


                    }
                        
                    
                
                    ):<tr></tr>}
                </tbody>
            </table>
        </div>
        
    </>)
}


export default Home