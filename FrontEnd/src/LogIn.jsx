import './App.css'
import './LogIn.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
import { useState } from 'react'

function LogIn(){
    let [openEye,toggleEye]=useState(false)
    return (<>
        <div className="container">
            <div className="central card-sm-1 card border-primary bg-primary">
                <div className="card-header" >
                    HELLO WORLD
                </div>
                <div className="card-body bg-white">
                    <form className="form">
                        <p className="s-0">
                           <label htmlFor="EmailUsername">Enter Email/Username </label><br></br>
                            <input type="text" id="EmailUsername" className="bg-white text-dark" />    
                        </p> 
                        <p className="s-0">
                            <label htmlFor="Password">Enter Password </label><br></br>
                            <input type={openEye?"text":"password"} id="Password" className="bg-white text-dark" />
                            <i className={openEye? "bi bi-eye":"bi bi-eye-slash"} id="togglePassword"
                            onClick={()=>toggleEye(ope=>!ope)}></i>
                        </p>
                        <br></br>
                        <input type="submit" className="bg-primary" value="LogIn" />
                    </form>
                </div>
            </div>
        </div>
    
    
    
    
    </>)
}

export default LogIn