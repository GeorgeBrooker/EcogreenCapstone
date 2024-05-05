import React, { createContext, useState } from "react";
import all_product from '../Components/Assets/all_product'; //remove this line when de-comment this file for front and back connection

export const ShopContext = createContext(null);
const serverUri = "http://localhost:3000";
const getDefaultCart = ()=>{
    let cart = {};
    for (let index = 0; index < all_product.length+1; index++){
        cart[index] = 0;
    }
    return cart;
}

const checkLogin = ()=>{
    fetch(serverUri + "/api/shop/CheckLogin", {
        method: "GET",
        headers: {
            'Authorization': 'Basic ' + btoa(localStorage.getItem("email") + ":" + localStorage.getItem("pass")),
            'accept': 'text/plain'
        }
    })
        .then(response => {
            if (response.status >= 400) {
                console.log("Login failed");
                return false
            }

            if (response.status === 200 || response.status === 204) {
                console.log("Login successful");
                return true
            }
        })
}
const logout = ()=>{
    localStorage.setItem("email", "");
    localStorage.setItem("pass", "");
    checkLogin()
}
const ShopContextProvider = (props)=> {

    // const [all_product,setAll_Product] = useState([]);
    const [cartItems,setCarItems] = useState(getDefaultCart());

    // useEffect(()=>{
    //     fetch('http://localhost:4000/allproducts')
    //     .then((response)=>response.json())
    //     .then((data)=>setAll_Product(data))

    //     if(localStorage.getItem('auth-token')){
    //         fetch('http://localhost:4000/getcart',{
    //             method:'POST',
    //             headers:{
    //                 Accept:'application/form-data',
    //                 'auth-token':`${localStorage.getItem('auth-token')}`,
    //                 'Content-Type':'application.json',
    //             },
    //             body:'',
    //         }).then((response)=>response.json()).then((data)=>setCarItems(data));
    //     }
    // },[])
    
    
    const addToCart = (itemId) =>{
        setCarItems((prev)=>({...prev,[itemId]:prev[itemId]+1}));
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
    }

    const removeFromCart = (itemId) =>{
        setCarItems((prev)=>({...prev,[itemId]:prev[itemId]-1}))
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
    }
     
    const updateCart = (itemId, quantity) => {
        if (quantity >= 0) {
            setCarItems((prev) => ({
                ...prev,
                [itemId]: quantity,
            }));
        }
    };

    const getTotalCartAmount =() =>{
        let totalAmount = 0;
        for(const item in cartItems )
        {
            if(cartItems[item]>0)
            {
                let itemInfo = all_product.find((product)=>product.id === Number(item))
                totalAmount += itemInfo.new_price * cartItems[item];
            }
             
        }
        return totalAmount;
    }
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
            }
        
    
    const contextValue = {serverUri, checkLogin, logout, updateCart, getTotalCartItems, getTotalCartAmount,all_product,cartItems,addToCart,removeFromCart};

    return (
        <ShopContext.Provider value={contextValue}>
            {props.children}
        </ShopContext.Provider>
    )
}

export default ShopContextProvider;