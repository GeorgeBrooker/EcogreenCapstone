import React, { createContext, useState } from "react";
export const ShopContext = createContext(null);
const serverUri = "http://localhost:3000";

const all_products = async ()=>{
    const response = await fetch(serverUri + "/api/shop/GetAllStock", {
        method: "GET",
        headers: {
            'accept': 'application/json'
        }
    })
    const data = await response.json();
    const mappedProucts = data.map(product => {
        let uriName = product.name.replace(/\s/g, '-');
        let productUri = `${product.photoUri}${uriName}-${product.id}/`;
        
        let discountedPrice = Number(product.price) * (1 - (Number(product.discountPercentage)/100));
        let roundedPrice = Math.round(discountedPrice / 0.05) * 0.05;
        
        return {
            id: product.id,
            name: product.name,
            image: productUri + "1.jpeg",
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

const checkLogin = async ()=>{
    const token = localStorage.getItem('auth-token');
    if (!token) {
        console.log('No token found');
        return false;
    }

    const response = await fetch(serverUri + '/api/auth/ValidateCustomer', {
        method: "GET",
        headers: {
            'Authorization': 'Bearer ' + token,
        },
    });
    if (response.ok) {
        const userSession = await response.json();
        sessionStorage.setItem('Email', userSession.Email);
        sessionStorage.setItem('Fname', userSession.Fname);
        sessionStorage.setItem('Lname', userSession.Lname);
        sessionStorage.setItem('Id', userSession.Id);
        
        console.log('User session restored', userSession);
        return true
    } else {
        console.error('Failed to restore user session');
        return false
    }
}

// TODO update this to work with new login system
const logout = ()=>{
    localStorage.removeItem('auth-token');
    sessionStorage.clear();
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
    
    const getTotalCartItems =() =>{
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
    
    
    const contextValue = {serverUri, checkLogin, logout, updateCart, getTotalCartItems, getTotalCartAmount, all_product,cartItems,addToCart,removeFromCart};

    return (
        <ShopContext.Provider value={contextValue}>
            {props.children}
        </ShopContext.Provider>
    )
}

export default ShopContextProvider;