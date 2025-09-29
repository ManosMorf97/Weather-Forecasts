import './App.css'
import './styles.css'
import './CreateAccount.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.min.css'
import {Link} from "react-router-dom"
import { useState } from 'react'
//import { useState } from 'react'
import API_KEY from "./cityApi.js"

function CreateAccount(){
    //let [openEye,toggleEye]=useState(false)
    let [theirForecasts,setTheirForecasts]=useState([])
    let forecasts=["VisualCorssing","AccuWeather","WeatherApi","WeatherBit","Meteo"]
    let jsxForecasts=forecasts.map(x=><option 
        onClick={()=>!theirForecasts.includes(x)?setTheirForecasts([...theirForecasts,x]):""}key={x}>{x}</option>)
    let jsxTheirForecasts=theirForecasts.map(x=><li key={x}>{x}</li>)
    let city_url=`https://api.api-ninjas.com/v1/city?X-Api-key=${API_KEY}&name=`
    //fetch(city_url).then((res)=>console.log(res.json()))
    let [city_res,setCityRes]=useState("Athens")
    let [theirCities,setTheirCities]=useState([])
    let jsxTheirCities=theirCities.map(x=><option 
        onClick={()=>setTheirCities([...theirCities,x])}key={x}>{x}</option>)

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
                        <label htmlFor="forecasts">Choose Forecast Sites</label>
                        <select id="forecasts" defaultValue="--select forecast sites--">
                            <option disabled value="--select forecast sites--">--select forecast sites--</option>
                            {jsxForecasts}
                        </select>
                        <ul>
                            {jsxTheirForecasts}
                        </ul>
                        <br></br>
                        <label htmlFor="Cities">Choose Cities</label>
                        <select id="cities"  defaultValue="--type interesting cities--"onChange=
                        {(e)=>{console.log(e.target.value);fetch(city_url+e.target.value).then((res)=>console.log(res.json()))}}>
                            <option disabled value="--type interesting cities--" >--type interesting cities--</option>
                            <option key={city_res}>{city_res}</option>
                        </select>
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