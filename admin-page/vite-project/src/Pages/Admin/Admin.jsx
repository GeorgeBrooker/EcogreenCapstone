import React from "react";
import './Admin.css'
import Sidebar from "../../Components/Sidebar/Sidebar";
import { Routes, Route } from "react-router-dom";
import AddProduct from "../../Components/AddProduct/AddProduct";
import ListProduct from "../../Components/ListProduct/ListProduct";
import ListCustomers from "../../Components/ListCustomers/ListCustomers";
import ListOrder from "../../Components/ListOrder/ListOrder";
import { Drawer, IconButton, List, ListItem, ListItemText } from "@mui/material";
import MenuIcon from '@mui/icons-material/Menu';


const Admin = () =>{
  return(
    <div className="admin">
        <Sidebar/>
        <div className={"page-content"}>
            <Routes>
                <Route path="/addproduct" element={<AddProduct/>}/>
                <Route path="/listproduct" element={<ListProduct/>}/>
                <Route path="/listcustomers" element={<ListCustomers/>}/>
                <Route path="/listorder" element={<ListOrder/>}/>
            </Routes>
        </div>
    </div>
  )
}
export default Admin