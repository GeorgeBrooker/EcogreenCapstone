import React, { useContext, useState } from "react";
import './ProductDisplay.css'
import { ShopContext } from "../../Context/ShopContext";
import star_icon from '../Assets/star_icon.png'
import star_dull_icon from '../Assets/star_dull_icon.png'
import { Link } from 'react-router-dom';

const ProductDisplay = (props) =>{
    const {product} = props;
    const {addToCart} = useContext(ShopContext);
    const [mainImage, setMainImage] = useState(product.image + "1.jpeg");


    const handleImageClick = (image) => {
        setMainImage(image);
    };

    const getImageClass = (image) => {
        return image === mainImage ? 'productdisplay-img-item selected' : 'productdisplay-img-item';
    };

    return(
        <div className="productdisplay">
            <div className="productdisplay-left">
                <div className="productdisplay-img-list">
                    <img className={getImageClass(product.image + "1.jpeg")} src={product.image + "1.jpeg"} alt=''onClick={() => handleImageClick(product.image + "1.jpeg")}/>
                    <img className={getImageClass(product.image + "2.jpeg")} src={product.image + "2.jpeg"} alt=''onClick={() => handleImageClick(product.image + "2.jpeg")}/>
                    <img className={getImageClass(product.image + "3.jpeg")} src={product.image + "3.jpeg"} alt=''onClick={() => handleImageClick(product.image + "3.jpeg")}/>
                    <img className={getImageClass(product.image + "4.jpeg")} src={product.image + "4.jpeg"} alt=''onClick={() => handleImageClick(product.image + "4.jpeg")}/>
                </div>
                <div className="productdisplay-img">
                    <img className='productdisplay-main-img' src={mainImage} alt='' />
                </div>

            </div>
            <div className="productdisplay-right">
                <h1>{product.name}</h1>
                
                <div className="productdisplay-right-stars">
                    <img src={star_icon} alt="" />
                    <img src={star_icon} alt="" />
                    <img src={star_icon} alt="" />
                    <img src={star_icon} alt="" />
                    <img src={star_dull_icon} alt="" />
                    <p>(111)</p>
                    </div>
                <div className="productdisplay-right-prices">
                    <div className="productdisplay-right-price-old">
                        ${product.old_price}
                    </div>
                    <div className="productdisplay-right-price-new">
                        ${product.new_price}
                    </div>
                </div>
                <div className="productdisplay-right-description">
                    {product.description}
                </div>
                <div className="buttons-box">
                <button onClick={()=>{addToCart(product.id)}}>Add to Cart</button>
                <Link to="/shop"><button>Back to Shop</button></Link>
                </div>
            </div>
        </div>
    )
}
export default ProductDisplay;