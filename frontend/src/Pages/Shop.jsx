import React, {useContext} from "react"
import { ShopContext } from "../Context/ShopContext"
import dropdown_icon from '../Components/Assets/dropdown_icon.png'
import Item from '../Components/Item/Item'
import './CSS/Shop.css'

const Shop = (props) => {
    const {all_product} = useContext(ShopContext);

    return (
        <div className="shop">
            <div className="shop-indexSort">
                <p>
                    <span>Showing 1-12</span> out of 36 products
                </p>
                <div className="shop-sort">
                    Sort by <img src={dropdown_icon} alt="" />
                </div>
            </div>
            <div className="shop-products">
                {all_product.map((item,i)=>{
                    return <Item key={i} id={item.id} name={item.name} image={item.image} new_price={item.new_price} old_price={item.old_price} />
                })}
            </div>
            <div className="shop-loadmore">
                Explore More
            </div>
        </div>
    )
}

export default Shop