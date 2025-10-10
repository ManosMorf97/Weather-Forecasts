import { useEffect,useState,useRef } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import LoadingSpinner from './LoadingSpinner.jsx'
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
            let resjson= await res.json()
            localStorage.setItem(storageNames.current[i],resjson)
        }
        activate(false)
    }

    var LogOut=(e)=>{
        e.preventDefault()
        for (let storageName of storageNames.current)
            localStorage.removeItem(storageName)
        localStorage.removeItem("UserLoggedIn")
        navigate('/logIn')
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
        <div className={(active?"sleeping":"")+" container top-cover z-1"}>
            <div className={"bg-primary text-light full-width background-card-color"}>
                <h1 >WELCOME {email}</h1>
                <nav className={(active?"sleeping":"")+"  navbar navbar-light "}>
                    <button className="navbar-toggler bg-white" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" 
                    aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                        <span className="navbar-toggler-icon text-white"></span>
                    </button>
                    <button className="bg-primary text-light">
                       <h6 >All Predictions</h6> 
                    </button>
                    <button className="bg-primary text-light">
                        <h6 >High Accuracy Predictions</h6>
                    </button>
                    <button className="bg-primary" >
                        <h4>
                            <i className="bi bi-bell" style={{"color":"white"}}></i>
                        </h4>
                    </button>
                    
                </nav>
                <button id="navbarNav" onClick={(e)=>LogOut(e)} 
                className={(active?"sleeping":" ")+" collapse bg-white text-primary border border-primary"} 
                style={{"float":"left","topPadding":"0px","display":active?"none ":" "}}>
                    LOG OUT
                </button>
            </div>
         
        </div>
    </>)
}


export default Home