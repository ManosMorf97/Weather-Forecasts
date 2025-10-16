async function updateStorage(controllers,storageNames,url){
    for(let i=0; i<controllers.length; i++){
        var res= await fetch(url+controllers[i],{
            method: "POST",
            mode: "cors",
            headers:{
                'Content-Type':'application/json'
            },
            body:JSON.stringify(localStorage.getItem("UserLoggedIn"))
        })
        let resjson= JSON.stringify(await res.json())
        localStorage.setItem(storageNames[i],resjson)
    }

}

export {updateStorage};