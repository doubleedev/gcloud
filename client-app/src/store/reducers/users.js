import * as actionTypes from '../actions/actionTypes'
import { updateObject } from '../../shared/utility'

const initialState = {
    users: [],
    something: ''
}

const usersResponse = (state, action) =>
    updateObject(state, {
        users: action.users === undefined ? state.users : action.users,
        something: action.something === undefined ? state.something : action.something
    })

const reducer = (state = initialState, action) => {
    switch (action.type) {
        case actionTypes.GET_SOMETHING_START:
        case actionTypes.GET_SOMETHING_SUCCESS:
        case actionTypes.GET_SOMETHING_FAIL:
        case actionTypes.FETCH_USERS_START:
        case actionTypes.FETCH_USERS_SUCCESS:
        case actionTypes.FETCH_USERS_FAIL:
            return usersResponse(state, action)
        default:
            return state
    }
}

export default reducer
