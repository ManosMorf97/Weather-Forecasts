import { useEffect,useState,useRef, useContext, createContext } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import LoadingSpinner from './LoadingSpinner.jsx'
import HomeNav from "./HomeNav.jsx";
import TableAllPredictions from "./TableAllPredictions.jsx";
import './styles.css'
import './decoration.css'
import { updateStorage } from "./utilities.js";

export const MyContext=createContext();

function Home(){
    
    let navigate=useNavigate()
    let [content,setContent]=useState({active:true,predictions:[]})
    //let [active,activate]=useState(true)
    let [email,setEmail]=useState("")
    //let [predictions,setPredictions]=useState([])
    let storageNames=useRef(["usersPredictions","usersSuggestions","usersNotification","usersCountNotification"])
    var fetchUserData=async (url)=>{
        
        let emptyStorage=false
        for (let storageName of storageNames.current)
            if(localStorage.getItem(storageName)==null)
                emptyStorage=true
        if(!emptyStorage){
            setContent({...content,active:false,predictions:JSON.parse(localStorage.getItem(storageNames.current[0]))})
            return
        }
        let controllers=["Predictions/","Suggestions/","Notifications/","Notifications/GetCountNotifications/"]
        await updateStorage(controllers,storageNames.current,url)
        setContent({...content,active:false,predictions:JSON.parse(localStorage.getItem(storageNames.current[0]))})
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
         <LoadingSpinner active={content.active} />
        <div>
            <MyContext.Provider value={content}>
                <HomeNav email={email}/>
            </MyContext.Provider>
            
            
        </div>
        
    </>)
}


export default Home