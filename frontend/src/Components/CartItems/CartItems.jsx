import React, { useContext, useState } from "react";
import './CartItems.css'
import { ShopContext } from "../../Context/ShopContext";
// import remove_icon from '../Assets/remove_icon.png';
// import add_icon from '../Assets/add_icon.png';
import QuantityStepper from "../QuantityStepper/QuantityStepper";
import AddressForm from "../AddressForm/AddressForm";

//TODO move this kind of logic into the main Cart component this is just for testing
const CartItems =()=>{
    const {getTotalCartAmount, all_product, cartItems, removeFromCart,addToCart, proceedToCheckout} = useContext(ShopContext)
    const [paymentType, setPaymentType] = useState("");
    const [deliveryType, setDeliveryType] = useState("");

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
                                    <img src={e.image + "1.jpeg"} alt="" className="carticon-product-icon" />
                                    <p>{e.name}</p>
                                    <p>{e.new_price}</p>
                                    <div className="quantity-control">
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
            <div className="address-form"><AddressForm/></div>
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
                    
                    <div className="dropdowns">
                        <div className="payment-div">
                        <label htmlFor="paymentType">Payment Type:</label>
                        <select
                            id="paymentType"
                            value={paymentType}
                            onChange={(e) => setPaymentType(e.target.value)}
                        >
                            <option value="" disabled>Select payment type</option>
                            <option value="creditCard">Credit Card</option>
                            <option value="paypal">PayPal</option>
                        </select>
                        </div>
                        <div className="delivery-div">
                        <label htmlFor="deliveryType">Delivery Type:</label>
                        <select
                            id="deliveryType"
                            value={deliveryType}
                            onChange={(e) => setDeliveryType(e.target.value)}
                        >
                            <option value="" disabled>Select delivery type</option>
                            <option value="standard">NZ Post</option>
                            {/* <option value="express">Express Delivery</option> */}
                        </select>
                        </div>
                    </div>
                        
                        <div className="proceed-div">
                            <button className="proceed-button" onClick={proceedToCheckout}>Proceed to Checkout</button>
                        </div>
                    
                </div>

            </div>
        </div>
    );
}
export default CartItems