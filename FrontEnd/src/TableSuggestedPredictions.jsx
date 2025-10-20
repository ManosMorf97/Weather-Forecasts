import { useEffect,useState } from "react"

function TableSuggestedPredictions(){
    let [suggestedPredictions,setSuggestedPredictions]=useState([])
    useEffect(()=>{
        setSuggestedPredictions(JSON.parse(localStorage.getItem("usersSuggestions")))
    },[])
    return(
        <table className={"table-centered table table-success table-bordered"}>
                <thead>
                    <tr>
                        <th>Site Name</th>
                        <th>City Name</th>
                        <th>Date</th>
                        <th>Time</th>
                        <th>DayNight</th>
                        <th>Temperature</th>
                        <th>Temperature Feels</th>
                        <th>Wind Speed</th>
                        <th>Sky Condition</th>
                        <th>Humidity</th>
                        <th>People Interested</th>
                        <th>Total Ratings</th>
                        <th>Average Rating</th>
                    </tr>
                    
                </thead>
                <tbody>
                    {suggestedPredictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time)).
                    length>0?
                        suggestedPredictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time))
                        .map((x,index)=>{
                            let weather=JSON.parse(x.weather)
                            return(
                            <tr key={index}>
                                <td>{x.site_name}</td>
                                <td>{x.city_name}</td>
                                <td>{x.date}</td>
                                <td>{x.time}</td>
                                <td>{weather.dayNight}</td>
                                <td>{weather.temperature +'°C'}</td>
                                <td>{weather.feelsLike+ '°C'}</td>
                                <td>{weather.windSpeed}</td>
                                <td>{weather.skyCondition}</td>
                                <td>{weather.humidity}</td>
                                <td>{x.people_Interested}</td>
                                <td>{x.total_Ratings}</td>
                                <td>{x.max_Rating}</td>
                            </tr>
                            )
                        }
                            
                        
                    
                        ):<tr></tr>
                    }
                </tbody>
            </table>
    )
}

export default TableSuggestedPredictions