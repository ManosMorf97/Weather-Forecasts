import { useEffect,useState } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import LoadingSpinner from './LoadingSpinner.jsx'
import './styles.css'
import './decoration.css'

function Home(){
    
    let navigate=useNavigate()
    let [active,activate]=useState(true)
    let user=null
    var fetchUserData=async (url)=>{
        let storageNames=["usersPredictions","usersSuggestions","usersNotification","usersCountNotification"]
        let emptyStorage=false
        for (let storageName of storageNames)
            if(localStorage.getItem(storageName)==null)
                emptyStorage=true
        if(!emptyStorage){
            activate(false)
            return
        }
            
        let email=localStorage.getItem("UserLoggedIn")
        let controllers=["Predictions/","Suggestions/","Notifications/","Notifications/GetCountNotifications/"]
        for(let i=0; i<controllers.length; i++){
            var res= await fetch(url+controllers[i],{
                method: "POST",
                mode: "cors",
                headers:{
                    'Content-Type':'application/json'
                },
                body:JSON.stringify(email)
            })
            let resjson= await res.json()
            localStorage.setItem(storageNames[i],resjson)
        }
        activate(false)
    }
    
    useEffect(()=>{
        console.log(localStorage.getItem("UserLoggedIn"))
        user=localStorage.getItem("UserLoggedIn")
        if(user==null)
            navigate('/logIn')
        else
            fetchUserData(url)
    },[navigate])
    
    return(<>
        <LoadingSpinner active={active}/>
        <h1 className={active?"sleeping":""}>HELLO {user}</h1>
    </>)
}


export default Home