import { BrowserRouter as Router, Routes, Route} from "react-router-dom";
import { useState,useEffect } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'

import 'bootstrap/dist/css/bootstrap.min.css'
import LogIn from './LogIn.jsx'
import CreateAccount from './CreateAccount.jsx'
let pages={'LogIn':<LogIn/>,
           'CreateAccount':<CreateAccount/>
}
function App() {
  const [count, setCount] = useState(0)
  let page="LogIn"
  useEffect(()=>{},[page]);
  //return(pages[page])
  return (
    <>
    <Router>
      <Routes>
        <Route path="/createAccount" element={<CreateAccount/>} />
        <Route path="/" element={<LogIn/>}/>
      </Routes>
    </Router>
    </>
  )
}

export default App
