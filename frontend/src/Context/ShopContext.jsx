import React, { createContext, useState } from "react";
import { checkLogin } from './UserContext';
export const ShopContext = createContext(null);
export const serverUri = process.env.NODE_ENV === 'production' 
    ? "https://nn8hvsrhhk.execute-api.ap-southeast-2.amazonaws.com" 
    : "http://localhost:3000";
// export const serverUri ="http://127.0.0.1:3000";
const cleanProductName = (productName) => {
    const supportedCharsRegex = /[^a-zA-Z0-9!,_.*'()]/g; // Match any character that is not in the supported set
    const cleanedName = productName.replace(supportedCharsRegex, '-');
    
    return cleanedName;
}
const all_products = async ()=>{
    const response = await fetch(serverUri + "/api/shop/GetStockForSale", {
        method: "GET",
        headers: {
            'accept': 'application/json'
        }
    })
    const data = await response.json();
    const mappedProucts = data.map(product => {
        let uriName = cleanProductName(product.name);
        let productUri = `${product.photoUri}${uriName}-${product.id}/`;
        
        let discountedPrice = Number(product.price) * (1 - (Number(product.discountPercentage)/100));
        let roundedPrice = Math.round(discountedPrice / 0.05) * 0.05;
        
        return {
            id: product.id,
            name: product.name,
            image: productUri,
            old_price: product.price,
            
            // Ensures string displays in 4 digit 2dp format, rounded to nearest 5 cents. 
            // This is aestheticly pleasing ana accurate enough (cast with Number() when calculating)
            new_price: roundedPrice.toFixed(2),
            description: product.description
        };
    });
    return mappedProucts;
}
// TODO find a way to have this more elegantly placed, perhaps move all cosnt declarations to the bottom?
// TODO inspect the best way to manage this API call, at the moment this slows down the load of the site significantly.
const all_product = await all_products();
const getDefaultCart = ()=>{
    let cart = {};
    for(const product of all_product){
        cart[product.id] = 0;
        console.log(product.name, product.id);
    }
    console.log(cart);
    return cart;
}

// TODO update this to work with new login system
const logout = ()=>{
    localStorage.removeItem('auth-token');
    sessionStorage.clear();
    checkLogin();
    // TODO SET STATE TO LOGGED OUT
}
const ShopContextProvider = (props)=> {

    // const [all_product,setAll_Product] = useState([]);
    const [cartItems,setCartItems] = useState(getDefaultCart());
    
    const addToCart = (itemId) =>{
        console.log("addToCart")
        setCartItems( (prev)=>( {...prev,[itemId]:prev[itemId]+1} ) );
        //
        // TODO IF LOGGED IN POST UPDATE TO SERVER
        //
        // if(localStorage.getItem('auth-token')){
        //     fetch('http://localhost:4000/addtocart',{
        //         method:'POST',
        //         headers:{
        //             Accept:'application/form-data',
        //             'auth-token':`${localStorage.getItem('auth-token')}`,
        //             'Content-Type':'application/json',
        //         },
        //         body:JSON.stringify({'itemId':itemId}),
        //     })
        //     .then((response)=>response.json())
        //     .then((data)=>console.log(data));
        // }
    };

    const removeFromCart = (itemId) =>{
        setCartItems((prev)=>({...prev,[itemId]:prev[itemId]-1}))
        // if(localStorage.getItem('auth-token')){
        //     fetch('http://localhost:4000/removefromcart',{
        //         method:'POST',
        //         headers:{
        //             Accept:'application/form-data',
        //             'auth-token':`${localStorage.getItem('auth-token')}`,
        //             'Content-Type':'application/json',
        //         },
        //         body:JSON.stringify({'itemId':itemId}),
        //     })
        //     .then((response)=>response.json())
        //     .then((data)=>console.log(data));
        // }
    };
     
    const updateCart = (itemId, quantity) => {
        if (quantity >= 0) {
            setCartItems((prev) => ({
                ...prev,
                [itemId]: quantity,
            }));
        }
        // TODO If logged in update the cart on the server
    };
    
    const getTotalCartAmount =() =>{
        let totalAmount = 0;
        for(const key in cartItems )
        {
            console.log(key, cartItems[key]);
            if(cartItems[key]>0)
            {
                let product = all_product.find((e)=>{
                    console.log(e.id);
                    console.log(e.id === key);
                    return e.id === key;
                });// TODO investigate better way to do this, Could we store all products in a dict and map to them?
                totalAmount += product.new_price * cartItems[key];
            }
        }
        return totalAmount;
    };
    
    const getTotalCartItems = ()=>{
            let totalItem = 0;
            for(const item in cartItems)
            {
                if(cartItems[item]>0)
                {
                    totalItem += cartItems[item];
                }
            }
            return totalItem;
        };

    // TODO add logic to check for customer login, if not logged in prompt them to login or go though guest checkout. This obviously neccecitates adding guest checkout logic too.
    const proceedToCheckout = async ()=>{
        console.log("Proceed to checkout activated");
        if (!sessionStorage.getItem('Id')) {alert("Please login to proceed to checkout"); return}
        
        // First create orderJson object
        const orderInput = {
            PaymentId: "AfakePaymentId", // Pretty sure I can remove this value now, or replace it with some other logic at least.
            CustomerId: sessionStorage.getItem('Id'),
            CustomerAddress: "AFakeAdress", // Get this value from the 3rd party hosted checkout page.
            OrderStatus: "Pending",
            PaymentType: "Stripe" // TODO add support for other payment types on checkout page.
        }
        
        // Then create list of stockRequest objects
        const lineItems = [];
        for (const key in cartItems) {
            if (cartItems[key] > 0) {
                lineItems.push({
                    ProductId: key,
                    Quantity: cartItems[key]
                })}
        }
        
        //Then send the data to the server,
        const response = await fetch(serverUri + '/api/shop/ProcessCheckout', {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                "Order": orderInput,
                "StockRequests": lineItems
            }),
        });
        console.log("Winner winner chicken dinner");
        //Await response and then redirect to the checkout page
        if (!response.ok) { // Handle errors here, we expect a 303 redirect to the payment page on success.
            console.error("Failed to process checkout", response.status, response.statusText);
            return
        }
        const data = await response.json();
        console.log("Success! Redirecting to checkout page.")
        window.location.href = data.redirectUrl;
    };
    
    const contextValue = {serverUri, checkLogin, logout, updateCart, getTotalCartItems, getTotalCartAmount, all_product, cartItems, addToCart, removeFromCart, proceedToCheckout};

    return (
        <ShopContext.Provider value={contextValue}>
            {props.children}
        </ShopContext.Provider>
    )
}

export default ShopContextProvider;