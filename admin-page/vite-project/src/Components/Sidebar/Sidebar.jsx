import './Sidebar.css'
import React, {useState} from "react";
import { NavLink } from "react-router-dom"; // Import NavLink
import add_product_icon from '../../assets/cart_icon1.png'
import list_product_icon from '../../assets/select.png'
import list_customers_icon from '../../assets/login.png'
import list_order_icon from '../../assets/package.png'

export const Sidebar = () => {
    return(
    <div className="sidebar">
        <NavLink to='/listproduct' activeClassName="active" style={{ textDecoration: "none" }}>
            <div className="sidebar-item">
                <img src={list_product_icon} alt="Product List"/>
                <p>Product List</p>
            </div>
        </NavLink>
        <NavLink to='/listcustomers' activeClassName="active" style={{ textDecoration: "none" }}>
            <div className="sidebar-item">
                <img src={list_customers_icon} alt="Customers List"/>
                <p>Customers List</p>
            </div>
        </NavLink>
        <NavLink to='/listorder' activeClassName="active" style={{ textDecoration: "none" }}>
            <div className="sidebar-item">
                <img src={list_order_icon} alt="All Order List"/>
                <p>All Order List</p>
            </div>
        </NavLink>
    </div>
    );
} 

export default Sidebar;
