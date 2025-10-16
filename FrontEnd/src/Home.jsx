import { useEffect,useState,useRef } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import LoadingSpinner from './LoadingSpinner.jsx'
import HomeNav from "./HomeNav.jsx";
import TableAllPredictions from "./TableAllPredictions.jsx";
import './styles.css'
import './decoration.css'
import { updateStorage } from "./utilities.js";
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
        await updateStorage(controllers,storageNames.current,url)
        setPredictions(JSON.parse(localStorage.getItem(storageNames.current[0])))
        
        activate(false)
    }

   
    useEffect(()=>{
                console.log("RENE")
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
            <TableAllPredictions active={active} predictions={predictions}/>
        </div>
        
    </>)
}


export default Home