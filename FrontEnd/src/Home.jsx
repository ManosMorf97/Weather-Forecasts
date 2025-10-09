import { useEffect,useState } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import LoadingSpinner from './LoadingSpinner.jsx'
import './styles.css'
import './decoration.css'

function Home(){
    
    let navigate=useNavigate()
    let [active,activate]=useState(true)
    let [email,setEmail]=useState("")
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
            let resjson= await res.json()
            localStorage.setItem(storageNames[i],resjson)
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
        <LoadingSpinner active={active}/>
        <div className="container">
            <nav className={(active?"sleeping":"")+" top-cover bg-primary z-1 navbar bg-primary"}>HELLO {email}</nav>
        </div>
    </>)
}


export default Home