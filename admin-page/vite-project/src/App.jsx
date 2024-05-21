import React from "react";
import Navbar from "./Components/Navbar/Navbar";
import Admin from './Pages/Admin/Admin'
export const serverUri = "http://localhost:3000";
const App = () =>{
  return(
    <div>
      <Navbar></Navbar>
      <Admin/>




    </div>
  )
}
export default App