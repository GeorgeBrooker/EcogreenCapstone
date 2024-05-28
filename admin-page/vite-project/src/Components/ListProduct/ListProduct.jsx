import React, { useEffect, useState } from "react";
import './ListProduct.css';
import cross_icon from "../../assets/remove_.png";
import {serverUri} from "../../App.jsx";

const ListProduct = () => {
    const [allProducts, setAllProducts] = useState([]);

    useEffect(() => {
        const fetchInfo = async () => {
            const response = await fetch(`${serverUri}/api/shop/GetAllStock`);
            const data = await response.json();
            setAllProducts(data);
        };
        fetchInfo();
    }, []);

     
 
return (
    <div className="list-product">
        <h1>All Products List</h1>
        <div className="listproduct-format-main">
            <p>Product</p>
            <p>Title</p>
            <p>Old Price</p>
            <p>New Price</p>
            <p>Stock</p>
            <p>Remove</p>
        </div>
        <div className="listproduct-allproducts">
            {allProducts.map((product, index) => {
                 
                const uriName = product.name.replace(/\s/g, '-');
                const productUri = `${product.photoUri}${uriName}-${product.id}1.jpeg`;
                const discountedPrice = Number(product.price) * (1 - (Number(product.discountPercentage)/100));

                return (
                    <div key={index} className="listproduct-format">
                        <img src={productUri} alt="" className="listproduct-product-icon" />
                        <p>{product.name}</p>
                        <p>${product.price}</p>
                        <p>${discountedPrice}</p>
                        <p>{product.totalStock}</p>
                        <img src={cross_icon} alt="remove" className="listproduct-remove-icon" />
                    </div>
                );
            })}
        </div>
    </div>
);
        }


export default ListProduct;
