import './App.css'
import './styles.css'
import './LogIn.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
import { useState } from 'react'
import MD5 from './md5.js'
import url from './url.js'
import Alert from './Alert.jsx'
import LoadingSpinner from './LoadingSpinner.jsx'
import {Link} from "react-router-dom"
function LogIn(){
    let [openEye,toggleEye]=useState(false)
    let [email_Username,setEmailUsername]=useState("");
    let [password,setPassword]=useState("");
    let [response,setResponse]=useState(-1)
    let [message,setMessage]=useState("")
    let [active,activate]=useState(false)

    var storeUser=async (url)=>{
        if(email_Username.includes("@")){
            localStorage.setItem("UserLoggedIn",email_Username)
            return
        } 
        var res= await fetch(url+'Authentication/GetMyEmail',{
            method: "POST",
            mode:"cors",
            headers:{
                'Content-Type':'application/json'
            },
            body:JSON.stringify(email_Username)
        })
        let email=await res.json()
        localStorage.setItem("UserLoggedIn",email)
        

    }

    var signIn=async (e)=>{
        e.preventDefault()
        activate(ac=>!ac)
        console.log("NOW "+response)
        console.log(password)
        let data={}
        data['email_Username']=email_Username
        data['hashedPassword']=MD5(password)
        let current_url=url+'Authentication/SignIn'
        let res=await fetch(current_url,{
            method:'POST',
            mode:'cors',
            headers: {
                'Content-Type':'application/json'
            },
            body:JSON.stringify(data)
        })
       
        setResponse(()=>1*res.status)
        console.log(response)
        let res_message=await res.json()
        if (res.status==200)
            storeUser(url)
        setMessage(()=>res_message)
        activate(ac=>!ac)

    }
    return (<>
        <LoadingSpinner active={active}></LoadingSpinner>
        <div className={(active?"non-displayed ":" ")+"container "}>
            <Alert response={response} message={message}></Alert>
            <div className="central card-sm-1 card border-primary border-card background-card-color no-padding">
                <h1 className="border-header-margin bg-primary margin-header">Log In</h1>
                <div className="card-body  justify-content-center magrin-card ">
                    <form className="form" onSubmit={(e)=>signIn(e)}>
                        <p className="s-0">
                           <label htmlFor="EmailUsername" >Enter Email/Username </label><br></br>
                            <input type="text" id="EmailUsername" onChange={(e)=>{setEmailUsername(e.target.value); setResponse(()=>-1)}}
                            value={email_Username} className="bg-white text-dark" />    
                            &nbsp;&nbsp;&nbsp;&nbsp;
                        </p> 
                        <p className="s-0">
                            <label htmlFor="Password">Enter Password </label><br></br>
                            <input type={openEye?"text":"password"} id="Password"  value={password} 
                            onChange={(e)=>{setPassword(e.target.value); setResponse(()=>-1)}}
                            className="bg-white text-dark" />
                            <i className={openEye? "bi bi-eye":"bi bi-eye-slash"} id="togglePassword"
                            onClick={()=>toggleEye(ope=>!ope)}></i>
                        </p>
                        <br></br>
                        <input type="submit" className="bg-primary" value="LogIn" />
                        <br></br>
                        <br></br>
                        <Link to='/createAccount'>Don't have an account. Sign Up</Link>
                    </form>
                </div>
            </div>
        </div>

    </>)
}

export default LogIn