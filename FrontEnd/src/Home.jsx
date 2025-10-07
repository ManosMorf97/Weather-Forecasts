import { useEffect } from "react";
import { useNavigate } from "react-router-dom";

function Home(){
    let navigate=useNavigate()
    let user=localStorage.getItem("UserLoggedIn")
    useEffect(()=>{
        if(user==null)
        navigate('/logIn')})
    
    return(<>
        <h1>HELLO {user}</h1>
    </>)
}


export default Home