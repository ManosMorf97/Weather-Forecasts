import 'bootstrap/dist/css/bootstrap.min.css'
function Alert(props){
    return (
        <div className={" z-2 alert alert-dismissible top-cover "+(props.response>=200&&props.response<300?"alert-success ":"alert-danger ")+
            (props.response==-1 ?"non-displayed ":"")} role="alert">
            {props.message}
            {//<button className="btn-close" aria-label="close" data-bs-dismiss="alert"></button>
            }
        </div>
    );
}

export default Alert