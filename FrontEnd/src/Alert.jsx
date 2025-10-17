import 'bootstrap/dist/css/bootstrap.min.css'
function Alert(props){
    return (
        <div className={" z-2 alert alert-dismissible top-cover "+(props.status>=200&&props.status<300?"alert-success ":"alert-danger ")+
            (props.status==-1 ?"non-displayed ":"")} role="alert">
            {props.message}
            {//<button className="btn-close" aria-label="close" data-bs-dismiss="alert"></button>
            }
        </div>
    );
}

export default Alert