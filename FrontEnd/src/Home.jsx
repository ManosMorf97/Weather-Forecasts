function Home(){

    let user=localStorage.getItem("UserLoggedIn")
    return(<h1>HELLO {user}</h1>)
}


export default Home