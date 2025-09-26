import './App.css'
import './styles.css'
import './CreateAccount.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
import {Link} from "react-router-dom"
//import { useState } from 'react'

function CreateAccount(){
    //let [openEye,toggleEye]=useState(false)
    return (<>
        <div className="container">
            <div className="central background-card-color no-padding">
                <h1 className="top-cover bg-primary">Create Account</h1>
                <div className="justify-content-center ">
                    <form className="form">
                        <p className="s-0">
                           <label htmlFor="Email" >Enter Email </label><br></br>
                            <input type="email" id="Emai" className="bg-white text-dark" />    
                            &nbsp;&nbsp;&nbsp;&nbsp;
                        </p>
                        <p className="s-0">
                           <label htmlFor="Username" >Enter Username </label><br></br>
                            <input type="text" id="Username" className="bg-white text-dark" />    
                        </p>  
                        <p className="s-0">
                            <label htmlFor="Password">Enter Password </label><br></br>
                            <input type="password" id="Password" className="bg-white text-dark" />
                        </p>
                        <p className="s-0">
                            <label htmlFor="repeatedPassword">Repeat Password </label><br></br>
                            <input type="password" id="repeatedPassword" className="bg-white text-dark" />
                        </p>
                        <br></br>
                        <input type="submit" className="bg-primary" value="Create Account" />
                        <br></br>
                        <br></br>
                        
                        <Link to="/">Already have an account. Sign In</Link>
                    </form>
                </div>
            </div>
        </div>
    </>)
}

export default CreateAccount