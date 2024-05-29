import React from "react"
import './CSS/OrderSuccess.css'
import { Link } from 'react-router-dom';
import ordersuccess from '../Components/Assets/ordersuccess.png'


const OrderSuccess = () => {
    return (
        <div className="container">
            <div className="infobox">
                <img src={ordersuccess} alt="order success" />
                <p>Order Success!</p>
                <Link to='/home'> <button>Back to Home</button> </Link>
            </div>
        </div>
    )
}

export default OrderSuccess