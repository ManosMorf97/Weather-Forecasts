import { useEffect,useState } from "react";

function Notifications(){

    let [notifications,setNotifications]=useState([])

    useEffect(()=>{
        setNotifications(JSON.parse(localStorage.getItem("usersNotification")))
    },[])

    return(
        notifications.length>0? 
            <table className={"table-centered table table-danger table-bordered"}>
                <thead>
                    <tr>
                        <th>Site Name</th>
                        <th>City Name</th>
                        <th>Date</th>
                        <th>Time</th>
                        <th>Description</th>
                    </tr>
                    
                </thead>
                <tbody>
                    {
                        notifications
                        .map((x,index)=>{
                            return(
                            <tr key={index}>
                                <td>{x.site_name}</td>
                                <td>{x.city_name}</td>
                                <td>{x.date}</td>
                                <td>{x.time}</td>
                                <td>{x.description}</td>
                            </tr>
                            )
                        }
                            
                        
                    
                        )
                    }
                </tbody>
            </table>
            :
            <h1>No warning so far</h1>
    )
}
export default Notifications