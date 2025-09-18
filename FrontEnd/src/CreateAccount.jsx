import './App.css'
import './LogIn.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
//import { useState } from 'react'

function CreateAccount(){
    //let [openEye,toggleEye]=useState(false)
    return (<>
        <div className="container">
            <div className="central card-sm-1 card border-primary border-card background-card-color no-padding">
                <h1 className="border-header-margin bg-primary margin-header">Create Account</h1>
                <div className="card-body  justify-content-center magrin-card ">
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
                        <a href="#">Already have an account. Sign In</a>
                    </form>
                </div>
            </div>
        </div>
    </>)
}

export default CreateAccount