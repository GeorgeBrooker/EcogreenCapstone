import React, {useContext, useState} from 'react'
import './Navbar.css'
import logo from '../Assets/logo.png'
import cart_icon from '../Assets/cart_icon1.png'
import { Link } from 'react-router-dom';
import { ShopContext } from '../../Context/ShopContext';


const Navbar = () => {

    const[menu, setMenu] = useState("/home");
    const {getTotalCartItems} = useContext(ShopContext)

    return (
        <div className='navbar'>
            <div className='nav-logo'>
                <img src={logo} alt='' />
                <p></p>
           
            </div>
            <ul className="nav-menu">
                <li onClick={()=>{setMenu("home")}}><Link style={{textDecoration: 'none'}} to='/home'>Home</Link>{menu==="home"?<hr />:<></> }</li>
                <li onClick={()=>{setMenu("shop")}}><Link style={{textDecoration: 'none'}} to='/shop'>Shop</Link>{menu==="shop"?<hr />:<></> }</li>
                <li onClick={()=>{setMenu("contactUs")}}><Link style={{textDecoration: 'none'}} to='/contactUs'>Contact Us</Link>{menu==="contactUs"?<hr />:<></> }</li>
            </ul>
            <div className="nav-login-cart">
                {localStorage.getItem('auth-token')?
                <button onClick={()=>{localStorage.removeItem('auth-token');window.location.replace('/home')}}>Logout</button>
                :<Link to='/login'><button>Login</button></Link>}
                <Link to='/cart'><img src={cart_icon} alt='' /></Link>
                <div className="nav-cart-count">{getTotalCartItems()}</div>
            </div>
        </div>
    )
}

export default Navbar