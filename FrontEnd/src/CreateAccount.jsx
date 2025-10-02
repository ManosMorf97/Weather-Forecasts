import './App.css'
import './styles.css'
import './CreateAccount.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
import {Link, useNavigate} from "react-router-dom"
import { useState } from 'react'
import MD5 from './md5.js'
import url from './url.js'
//import { useState } from 'react'
import API_KEY from "./cityApi.js"
import  Alert from './Alert.jsx'
import LoadingSpinner from './LoadingSpinner.jsx'

function CreateAccount(){
    //let [openEye,toggleEye]=useState(false)
    let navigate=useNavigate()
    let forecasts=["VisualCorssing","AccuWeather","WeatherApi","WeatherBit","Meteo"]
    let [active,activate]=useState(false)
    let [email,setEmail]=useState("")
    let [username,setUsername]=useState("")
    let [password,setPassword]=useState("")
    let [repeatedPassword,setRepeatedPassword]=useState("")
    let [theirForecasts,setTheirForecasts]=useState([])
    let [theirCities,setTheirCities]=useState([])
    let jsxForecasts=forecasts.map(x=><option 
        onClick={()=>!theirForecasts.includes(x)?setTheirForecasts([...theirForecasts,x]):""}key={x}>{x}</option>)
    let jsxTheirForecasts=theirForecasts.map(x=><li key={x}>{x}</li>)
    let city_url=`https://api.api-ninjas.com/v1/city?X-Api-key=${API_KEY}&name=`
    //fetch(city_url).then((res)=>console.log(res.json()))
    let jsxTheirCities=theirCities.map(x=><li key={x}>{x}</li>)
    /*let [response,setResponse]=useState(-1)
    let [message,setMessage]=useState("")*/
    let [response,setResponse]=useState({status:-1,message:""})
    async function postData(e){
        e.preventDefault()
        activate(ac=>!ac)
        let hashed_Password=MD5(password)
        let hashed_PasswordR=MD5(repeatedPassword)
        if(hashed_Password!=hashed_PasswordR){
            setResponse({...response,status:0,message:"Passwords do not match"})
            activate(ac=>!ac)
            return
        }
        if(password.length<=5){
            setResponse({...response,status:0,message:"Password too small"})
            activate(ac=>!ac)
            return
        }
        let current_url=url+'Authentication/SignUp'
        let data={'email':email,
            'username':username,
            'hashed_password':hashed_Password,
            'cityNames':theirCities,
            'siteNames':theirForecasts
        }
        let res=await fetch(current_url,{method:'POST',
            mode:'cors',
            headers:{
                'Content-Type':'application/json'
            },
            body:JSON.stringify(data)
        })
        let res_message=await res.json()
        setResponse({...response,status:res.status,message:res_message})
        activate(ac=>!ac)
        if (res.status==200){
            
            navigate("/logIn")
        }
            
    }

    return (<>
        <LoadingSpinner active={active}/>
        <div className={(active?"non-displayed ":" ")+"container "}>
            <Alert response={response.status} message={response.message} ></Alert>
            <div className="central background-card-color no-padding">
                <h1 className="top-cover bg-primary z-1">Create Account</h1>
                <div className="justify-content-center ">
                    <form className="form justify-content-center" method="POST" onSubmit={(e)=>postData(e)}>
                        <p className="s-0">
                           <label htmlFor="Email" >Enter Email </label><br></br>
                            <input type="email" id="Emai" value={email} className="bg-white text-dark" onChange={
                                (e)=>{response.status!=-1?setResponse({...response,status:-1}):{}; setEmail(e.target.value)}}/>    
                            &nbsp;&nbsp;&nbsp;&nbsp;
                        </p>
                        <p className="s-0">
                           <label htmlFor="Username" >Enter Username </label><br></br>
                            <input type="text" id="Username" value={username} className="bg-white text-dark" onChange={
                                (e)=>{response.status!=-1?setResponse({...response,status:-1}):{}; setUsername(e.target.value)}}/>    
                        </p>  
                        <p className="s-0">
                            <label htmlFor="Password">Enter Password </label><br></br>
                            <input type="password" id="Password" value={password} className="bg-white text-dark" onChange={
                                (e)=>{response.status!=-1?setResponse({...response,status:-1}):{}; setPassword(e.target.value)}}/>
                        </p>
                        <p className="s-0">
                            <label htmlFor="repeatedPassword">Repeat Password </label><br></br>
                            <input type="password" id="repeatedPassword" value={repeatedPassword} className="bg-white text-dark" onChange={
                                (e)=>{response.status!=-1?setResponse({...response,status:-1}):{}; setRepeatedPassword(e.target.value)}}/>
                        </p>
                        <br></br>
                        <label htmlFor="forecasts">Choose Forecast Sites</label>
                        <select id="forecasts" defaultValue="--select forecast sites--" onChange={
                                ()=>response.status!=-1?setResponse({...response,status:-1}):{}}>
                            <option disabled value="--select forecast sites--">--select forecast sites--</option>
                            {jsxForecasts}
                        </select>
                        <ul>
                            {jsxTheirForecasts}
                        </ul>
                        <br></br>
                        <label htmlFor="city">Choose Cities</label><br></br>
                        <input id="city"  type="text" placeholder="Type Cities you like" className="bg-white text-dark" onChange={
                            ()=>response.status!=-1?setResponse({...response,status:-1}):{}}/>
                        <br/>
                        <button onClick={async (e)=>{
                            e.preventDefault()
                            let res=await fetch(city_url+document.getElementById("city").value)
                            let city=await res.json()
                            if(city[0]===undefined){
                                setResponse(()=>({...res,status:0,message:"That city does not exist or is not available"}))
                                //setResponse(0) 
                                //setMessage("That city does not exist or it is not available")
                                console.log(response)
                            }
                            else{
                                setTheirCities([...theirCities,city[0].name]);
                                document.getElementById("city").value=""
                            }
                          
                        }}>Add City</button>
                        <ul>
                            {jsxTheirCities}
                        </ul>
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