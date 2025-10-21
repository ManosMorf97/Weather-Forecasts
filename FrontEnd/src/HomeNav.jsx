import {useContext, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import './styles.css'
import './decoration.css'
import TableAllPredictions from "./TableAllPredictions.jsx";
import {MyContext} from './Home.jsx'
import TableSuggestedPredictions from "./TableSuggestedPredictions.jsx";

function HomeNav(props){
    let navigate=useNavigate()
    let storageNames=useRef(["usersPredictions","usersSuggestions","usersNotification","usersCountNotification"])
    let [current_tab,setCurrentTab]=useState('all_predictions')
    let tabs=useRef({'all_predictions':<TableAllPredictions/>,'suggested_Predictions':<TableSuggestedPredictions/>,'Notifications':{}})
    var LogOut=(e)=>{
        e.preventDefault()
        for (let storageName of storageNames.current)
            localStorage.removeItem(storageName)
        localStorage.removeItem("UserLoggedIn")
        navigate('/logIn')
    }
    var changeTab=(e,new_current_tab)=>{
        e.preventDefault()
        if(current_tab!=new_current_tab){
            setCurrentTab(new_current_tab)
        }
    }
    let context=useContext(MyContext)
    return(
        <>
            <div className={(context.active?"sleeping":"")+" top-cover z-1"}>
                <div className={"bg-primary text-light full-width background-card-color"}>
                    <h1 >WELCOME {props.email}</h1>
                    <nav className={(context.active?"sleeping":"")+"  navbar navbar-light "}>
                        <button className="navbar-toggler bg-white" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" 
                        aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                            <span className="navbar-toggler-icon text-white"></span>
                        </button>
                        <button className={(current_tab==="all_predictions"?"bg-white text-primary":"bg-primary text-light")} 
                        onClick={(e)=>changeTab(e,"all_predictions")}>
                        <h6 >All Predictions</h6> 
                        </button>
                        <button className={(current_tab==="suggested_Predictions"?"bg-white text-primary":"bg-primary text-light")}
                        onClick={(e)=>changeTab(e,"suggested_Predictions")}>
                            <h6 >Suggested Predictions</h6>
                        </button>
                        <button className={current_tab==="Notifications"?"bg-white":"bg-primary"+" position-relative"} >
                            <div className="notification-badge bg-danger position-absolute mb-n6 float-right w-25" >3</div>
                            <br></br>
                            <h4>
                                <i className="bi bi-bell" style={{color:current_tab=="Notifications"?"blue":"white"}}>
                                </i>
                            </h4>
                        </button>
                        
                    </nav>
                    <button id="navbarNav" onClick={(e)=>LogOut(e)} 
                    className={(context.active?"sleeping":" ")+" collapse bg-white text-primary border border-primary"} 
                    style={{"float":"left","topPadding":"0px","display":context.active?"none ":" "}}>
                        LOG OUT
                    </button>
                </div>
            </div>
            {tabs.current[current_tab]}
        </>
    )
}

export default HomeNav