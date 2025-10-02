import { useNavigate } from "react-router-dom";

function Home(){
    let navigate=useNavigate()
    let user=localStorage.getItem("UserLoggedIn")
    if(user==null)
        navigate('/logIn')
    return(<>
        <h1>HELLO {user}</h1>
    </>)
}


export default Home