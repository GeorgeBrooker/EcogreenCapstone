import PropTypes from 'prop-types';
import './Modal.css'
import {IconButton} from "@mui/material";
import CancelIcon from '@mui/icons-material/Cancel';
import React from "react";
 
  

const Modal = ({ isOpen, children, onClose }) => {
    if (!isOpen) return null;

    return (
        <div className="modal-backdrop">
        <div className="modal-content">
            <button onClick={onClose} className="imageupload-delete">X</button>
            {children}
        </div>
        </div>
    );
};
Modal.propTypes = {
    isOpen: PropTypes.bool.isRequired,
    onClose: PropTypes.func.isRequired,
    children: PropTypes.node
  };

export default Modal;