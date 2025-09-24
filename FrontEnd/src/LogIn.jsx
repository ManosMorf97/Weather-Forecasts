import './App.css'
import './LogIn.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
import { useEffect, useState } from 'react'
import MD5 from './md5.js'
import url from './url.js'

function LogIn(){
    let [openEye,toggleEye]=useState(false)
    let [email_Username,setEmailUsername]=useState("");
    let [password,setPassword]=useState("");
    let [answer,setAnswer]=useState(-1)

    var signIn=(e)=>{
        setAnswer(0)
        e.preventDefault()
        console.log(password)
        let data={}
        data['email_Username']=email_Username
        data['hashedPassword']=MD5(password)
        let current_url=url+'Authentication/SignIn'
        fetch(current_url,{
            method:'POST',
            mode:'cors',
            headers: {
                'Content-Type':'application/json'
            },
            body:JSON.stringify(data)
        }).then( res=>{
            setAnswer(res.status)
            return res.json()})
        .then(res=>{
            console.log(res)
        })
    }
    return (<>
        <div className="container">
            <div className={"alert alert-dismissible alert-pos "+(answer>=200&&answer<300?"alert-success ":"alert-danger ")+
                (answer==-1 || answer==0 ?"alert-non-displayed ":"")} role="alert">
                HEY
                <button className="btn-close" aria-label="close" data-bs-dismiss="alert"></button>
            </div>
            <div className="central card-sm-1 card border-primary border-card background-card-color no-padding">
                <h1 className="border-header-margin bg-primary margin-header">Log In</h1>
                <div className="card-body  justify-content-center magrin-card ">
                    <form className="form" onSubmit={(e)=>signIn(e)}>
                        <p className="s-0">
                           <label htmlFor="EmailUsername" >Enter Email/Username </label><br></br>
                            <input type="text" id="EmailUsername" onChange={(e)=>setEmailUsername(e.target.value)}value={email_Username} className="bg-white text-dark" />    
                            &nbsp;&nbsp;&nbsp;&nbsp;
                        </p> 
                        <p className="s-0">
                            <label htmlFor="Password">Enter Password </label><br></br>
                            <input type={openEye?"text":"password"} id="Password"  value={password} 
                            onChange={(e)=>setPassword(e.target.value)}
                            className="bg-white text-dark" />
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