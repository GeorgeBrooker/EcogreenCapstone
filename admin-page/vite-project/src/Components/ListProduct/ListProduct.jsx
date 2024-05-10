import React, { useEffect, useState } from "react";
import './ListProduct.css'
import cross_icon from"../../assets/remove_.png"
const serverUri = "http://localhost:3000";

const ListProduct = () =>{
    const[allproducts,setAlProducts] = useState([]);
    const fetchInfo = async()=>{
        await fetch(serverUri + "/api/shop/GetAllStock")
        .then((res)=>res.json())
        .then((data)=>{setAlProducts(data)});
    }
    useEffect(()=>{
        fetchInfo();

    },[])
    /*const remove_product = async ()=>{
        await fetch('')
    }*/
  return(
    <div className="list-product">
        <h1>All Products List</h1>
        <div className="listproduct-format-main">
            <p>Product</p>
            <p>Title</p>
            <p>Old Price</p>
            <p>New Price</p>
            
            <p>Remove</p>
        </div>
        <div className="listproduct-allproducts">
            <hr/>
            {allproducts.map((product,index)=>{
                return <><div key={index} className="listproduct-format-main listproduct-formt">
                    <img src={product.img} alt="" className="listproduct-product-icon" />
                    <p>{product.name}</p>
                    <p>${product.price}</p>
                    <p>${product.new_price}</p>
                    <img src={cross_icon} alt="" className="listproduct-remove-icon" />
                </div>
                <hr />
                </>
            })}
        </div>
         



    </div>
  )
}
export default ListProduct