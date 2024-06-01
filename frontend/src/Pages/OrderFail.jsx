import React from "react"
import './CSS/OrderFail.css'
import { Link } from 'react-router-dom';
import orderfail from '../Components/Assets/orderfail.png'



const OrderFail = () => {
    return (
        <div className="container">
            <div className="infobox">
                <img src={orderfail} alt="order fail" />
                <p>Order Fail!</p>
                <Link to='/cart'> <button>Re-Checkout</button> </Link>
                <Link to='/home'> <button>Back to Home</button> </Link>
                
            </div>
        </div>
    )
}

export default OrderFail