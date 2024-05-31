import React from "react";
import './Navbar.css';
import navlog from '../../assets/logo.png'


const Navbar = () =>{
  return(
    <div className="navbar">
        <img src={navlog} alt="" className="nav-logo"></img>
        <h1>Admin Panel</h1>
    </div>
  )
}
export default Navbar;