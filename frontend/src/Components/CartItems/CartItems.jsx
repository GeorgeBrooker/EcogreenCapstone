import React, { useContext } from "react";
import './CartItems.css'
import { ShopContext } from "../../Context/ShopContext";
// import remove_icon from '../Assets/remove_icon.png';
// import add_icon from '../Assets/add_icon.png';
import QuantityStepper from "../QuantityStepper/QuantityStepper";

const CartItems =()=>{
    const {getTotalCartAmount, all_product, cartItems, removeFromCart,addToCart} = useContext(ShopContext)
    // const handleChange = (e, productId) => {
    //     const newValue = e.target.value;
    //     if (newValue === '' || isNaN(newValue)) {
    //         return;
    //     }
    //     const newQuantity = parseInt(newValue, 10);
    //     if (newQuantity >= 0) {
    //         updateCart(productId, newQuantity); 
    //     }
    // };

    return(
        <div className="cartitems">
            <div className="cartitems-format-main">
                <p>Products</p>
                <p>Title</p>
                <p>Price</p>
                <p>Quantity</p>
                <p>Total</p>
                
            </div>
            <hr />
            {all_product.map((e) => {
                if(cartItems[e.id]>0)
                {
                    return <div key={e.id}>
                                <div className="cartitems-format cartitems-format-main">
                                    <img src={e.image} alt="" className="carticon-product-icon" />
                                    <p>{e.name}</p>
                                    <p>{e.new_price}</p>
                                    <div className="quantity-control">
                                    {/* <img className='cartitems-modi-icon' src={add_icon} onClick={()=>{addToCart(e.id)}} alt=""   />
                                    <button className="cartitems-quantity">{cartItems[e.id]}</button>
                                    <input className="cartitems-quantity" type='text' min='1'  value={cartItems[e.id]} onChange={(event) => handleChange(event, e.id)} />
                                    <img className='cartitems-modi-icon' src={remove_icon} onClick={()=>{removeFromCart(e.id)}} alt=""   /> */}
                                        <QuantityStepper
                                            quantity={cartItems[e.id]}
                                            onIncrease={() => addToCart(e.id)}
                                            onDecrease={() => removeFromCart(e.id)}
                                        />
                                    </div>
                                    <p>${e.new_price*cartItems[e.id]}</p>
                                </div>
                                <hr />
                            </div>
                }
                return null;
            })}
            <div className="cartitems-down">
                <div className="cartitems-total">
                    <h1>Cart Totals</h1>
                    <div>
                        <div className="cartitems-total-item">
                            <p>Subtotal</p>
                            <p>${getTotalCartAmount()}</p>
                        </div>
                        <hr />
                        <div className="cartitems-total-item">
                            <p>Shipping Fee</p>
                            <p>Free</p>
                        </div>
                        <hr />
                        <div className="cartitems-total-item">
                            <h3>Total</h3>
                            <h3>${getTotalCartAmount()}</h3>
                        </div>
                    </div>
                    <button className="proceed-button">PROCEED TO CHECKOUT</button>
                </div>
                <div className="cartitems-promocode">
                    <p>If you have a promo code, enter it here</p>
                    <div className="cartitems-promobox">
                        <input type="text" placeholder="promo code"/>
                        <button>Submit</button>
                    </div>
                </div>
            </div>
        </div>
    )
}
export default CartItems