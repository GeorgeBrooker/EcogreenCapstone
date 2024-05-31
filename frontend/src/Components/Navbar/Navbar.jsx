import React, { useContext, useState, useRef } from 'react';
import './Navbar.css';
import logo from '../Assets/logo.png';
import cart_icon from '../Assets/cart_icon1.png';
import { Link } from 'react-router-dom';
import { ShopContext } from '../../Context/ShopContext';
import drop_down_icon from '../Assets/drop_down_icon.png'

const Navbar = () => {
    const [menu, setMenu] = useState("home");
    const { getTotalCartItems } = useContext(ShopContext);
    const menuRef = useRef();
    const dropdown = (e) => {
        menuRef.current.classList.toggle('nav-menu-visible');
        e.target.classList.toggle('open');
    }
 
    const isLoggedIn = localStorage.getItem('auth-token');

    return (
        <div className='navbar'>
            <div className='nav-logo'>
                <img src={logo} alt='Logo' />
            </div>
            <img className='nav-dropdown' onClick={dropdown} src={drop_down_icon} alt="" />
            <ul ref={menuRef} className="nav-menu">
                <li onClick={() => { setMenu("home") }}><Link style={{ textDecoration: 'none' }} to='/home'>Home</Link>{menu === "home" ? <hr /> : <></>}</li>
                <li onClick={() => { setMenu("shop") }}><Link style={{ textDecoration: 'none' }} to='/shop'>Shop</Link>{menu === "shop" ? <hr /> : <></>}</li>
                <li onClick={() => { setMenu("contactUs") }}><Link style={{ textDecoration: 'none' }} to='/contactUs'>Contact Us</Link>{menu === "contactUs" ? <hr /> : <></>}</li>
                {isLoggedIn && (
                    <li onClick={() => { setMenu("detail") }}><Link style={{ textDecoration: 'none' }} to='/detail'>Detail</Link>{menu === "detail" ? <hr /> : <></>}</li> // 只有登录后才显示
                )}
            </ul>
            <div className="nav-login-cart">
                {isLoggedIn ?
                    <button onClick={() => { localStorage.removeItem('auth-token'); window.location.replace('/home') }}>Logout</button>
                    : <Link to='/login'><button>Login</button></Link>}
                <Link to='/cart'><img src={cart_icon} alt='Cart' /></Link>
                <div className="nav-cart-count">{getTotalCartItems()}</div>
            </div>
        </div>
    )
}

export default Navbar;
