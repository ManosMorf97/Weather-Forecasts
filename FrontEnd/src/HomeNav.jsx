import {useContext, useRef } from "react";
import { useNavigate } from "react-router-dom";
import url from './url.js'
import './styles.css'
import './decoration.css'
import TableAllPredictions from "./TableAllPredictions.jsx";
import {MyContext} from './Home.jsx'

function HomeNav(props){
    let navigate=useNavigate()
    let storageNames=useRef(["usersPredictions","usersSuggestions","usersNotification","usersCountNotification"])
    var LogOut=(e)=>{
        e.preventDefault()
        for (let storageName of storageNames.current)
            localStorage.removeItem(storageName)
        localStorage.removeItem("UserLoggedIn")
        navigate('/logIn')
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
                className={(context.active?"sleeping":" ")+" collapse bg-white text-primary border border-primary"} 
                style={{"float":"left","topPadding":"0px","display":context.active?"none ":" "}}>
                    LOG OUT
                </button>
            </div>
        </div>
        <TableAllPredictions/>
        </>
    )
}

export default HomeNav