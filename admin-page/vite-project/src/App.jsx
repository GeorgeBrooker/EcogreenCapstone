import React from "react";
import Navbar from "./Components/Navbar/Navbar";
import Admin from './Pages/Admin/Admin'
export const serverUri = process.env.NODE_ENV === 'production'
    ? "https://sz2jaar6r0.execute-api.ap-southeast-2.amazonaws.com"
    : "http://localhost:3000";
const App = () =>{
  return(
    <div>
      <Navbar></Navbar>
      <Admin/>




    </div>
  )
}
export default App