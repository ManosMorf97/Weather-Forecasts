import './styles.css'
import './decoration.css'

function TableAllPredictions(props){
    return(
    <table className={(props.active?"non-displayed ":" ")+"table-centered table table-dark table-bordered z-1"}>
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
                <th>Day Or Night</th>
                <th>Humidity</th>
            </tr>
            
        </thead>
        <tbody>
            {props.predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time)).
            length>0?
            props.predictions.filter(x=>new Date(Date.now())<=new Date(x.date+'T'+x.time))
            .map((x,index)=>{
                let weather=JSON.parse(x.weather)
                return(
                <tr key={index}>
                    <td>{x.site_name}</td>
                    <td>{x.city_name}</td>
                    <td>{x.date}</td>
                    <td>{x.time}</td>
                    <td>{weather.dayNight}</td>
                    <td>{weather.temperature}</td>
                    <td>{weather.feelsLike}</td>
                    <td>{weather.windSpeed}</td>
                    <td>{weather.skyCondition}</td>
                    <td>{weather.humidity}</td>
                </tr>
                )
                


            }
                
            
        
            ):<tr></tr>}
        </tbody>
    </table>)
}

export default TableAllPredictions;