import React from "react";
import './Navbar.css';
import navlog from '../../assets/logo.png'
import { Button } from "@mui/material";
import { CircularProgress } from "@mui/material";
import { cognitoUser } from "../../auth.js";

const handleLogout = ()=>{
    cognitoUser.signOut();
    localStorage.clear();
    sessionStorage.clear()
    window.location = "/"
    userSession = null;
}
const Navbar = () =>{
  return(
    <div className="navbar">
        <div className={"nav-left"}>
            <img src={navlog} alt="" className="nav-logo"></img>
            <h1>Admin Panel</h1>
        </div>
        <Button style={{marginRight: "15px"}} variant={"contained"} color={"error"} onClick={handleLogout}>Logout</Button>
        
    </div>
  )
}
export default Navbar;