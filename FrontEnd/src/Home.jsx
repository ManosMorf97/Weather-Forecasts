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
    let storageNames=useRef(["usersPredictions","usersSuggestions","usersNotification","usersCountNotification"])
    var fetchUserData=async (url)=>{
        
        let emptyStorage=false
        for (let storageName of storageNames.current)
            if(localStorage.getItem(storageName)==null)
                emptyStorage=true
        if(!emptyStorage){
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
            
    },[navigate])
    
    return(<>
        <LoadingSpinner active={active} />
        <HomeNav active={active} email={email}/>
    </>)
}


export default Home