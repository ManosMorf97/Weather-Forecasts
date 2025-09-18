import './App.css'
import './LogIn.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
import { useState } from 'react'

function LogIn(){
    let [openEye,toggleEye]=useState(false)
    return (<>
        <div className="container">
            <div className="central card-sm-1 card border-primary border-card background-card-color no-padding">
                <h1 className="border-header-margin bg-primary margin-header">Log In</h1>
                <div className="card-body  justify-content-center magrin-card ">
                    <form className="form">
                        <p className="s-0">
                           <label htmlFor="EmailUsername" >Enter Email/Username </label><br></br>
                            <input type="text" id="EmailUsername" className="bg-white text-dark" />    
                            &nbsp;&nbsp;&nbsp;&nbsp;
                        </p> 
                        <p className="s-0">
                            <label htmlFor="Password">Enter Password </label><br></br>
                            <input type={openEye?"text":"password"} id="Password" className="bg-white text-dark" />
                            <i className={openEye? "bi bi-eye":"bi bi-eye-slash"} id="togglePassword"
                            onClick={()=>toggleEye(ope=>!ope)}></i>
                        </p>
                        <br></br>
                        <input type="submit" className="bg-primary" value="LogIn" />
                        <br></br>
                        <br></br>
                        <a href="#">Don't have an account. Sign Up</a>
                    </form>
                </div>
            </div>
        </div>
    </>)
}

export default LogIn